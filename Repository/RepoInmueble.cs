using MySqlConnector;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoInmueble
    {
        private readonly string _connectionString;

        // Recibe IConfiguration para leer la cadena desde appsettings.json
        public RepoInmueble(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        // 🔹 ALTA 
        public int Alta(Inmueble i)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"INSERT INTO Inmuebles 
                         (Direccion, TipoId, MetrosCuadrados, Precio, PropietarioID, Activo)
                         VALUES (@dir, @tipoId, @m2, @precio, @prop, @activo);
                         SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dir", i.Direccion);
            command.Parameters.AddWithValue("@tipoId", i.TipoId);  // ✅ ahora usa el FK
            command.Parameters.AddWithValue("@m2", i.MetrosCuadrados);
            command.Parameters.AddWithValue("@precio", i.Precio);
            command.Parameters.AddWithValue("@prop", i.PropietarioId);
            command.Parameters.AddWithValue("@activo", i.Activo);

            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }

        // 🔹 OBTENER POR ID
        public Inmueble? Obtener(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio,
               i.PropietarioID, i.Activo,
               p.Nombre AS NombrePropietario,
               t.Nombre AS TipoNombre
        FROM Inmuebles i
        JOIN Personas p ON p.ID = i.PropietarioID
        JOIN Tipos_Inmuebles t ON i.TipoId = t.Id
        WHERE i.ID = @id";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    TipoNombre = reader.GetString("TipoNombre"), // 🔹 ahora viene del JOIN
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo")
                };
            }
            return null;
        }


        // 🔹 OBTENER TODOS
        public List<Inmueble> Obtener(bool incluirInactivos = false)
        {
            var lista = new List<Inmueble>();
            using var connection = new MySqlConnection(_connectionString);
            string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio, 
               p.Nombre AS NombrePropietario, i.PropietarioID, i.Activo,
               t.Nombre AS TipoNombre
        FROM Inmuebles i
        JOIN Personas p ON i.PropietarioID = p.ID
        JOIN Tipos_Inmuebles t ON i.TipoId = t.Id";

            if (!incluirInactivos)
                sql += " WHERE i.Activo = 1";

            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    TipoNombre = reader.GetString("TipoNombre"),  // 🔹 ahora correcto
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo")
                });
            }
            return lista;
        }

        // 🔹 OBTENER SOLO DISPONIBLES
        public List<Inmueble> ObtenerDisponibles()
        {
            var lista = new List<Inmueble>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio, 
               p.Nombre AS NombrePropietario, i.PropietarioID, i.Activo,
               t.Nombre AS TipoNombre
        FROM Inmuebles i
        JOIN Personas p ON i.PropietarioID = p.ID
        JOIN Tipos_Inmuebles t ON i.TipoId = t.Id
        WHERE i.Activo = 1
          AND NOT EXISTS (
              SELECT 1
              FROM Contratos c
              WHERE c.InmuebleID = i.ID
                AND UPPER(c.Estado) = 'VIGENTE'
                AND DATE(NOW()) BETWEEN DATE(c.FechaInicio) AND DATE(c.FechaFin)
          );";

            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    TipoNombre = reader.GetString("TipoNombre"), // ✅ ahora correcto
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo")
                });
            }
            return lista;
        }


        /// 🔹 OBTENER DISPONIBLES ENTRE DOS FECHAS 
        public List<Inmueble> ObtenerDisponiblesEntre(DateTime inicio, DateTime fin)
        {
            var lista = new List<Inmueble>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio,
               p.Nombre AS NombrePropietario, i.PropietarioID, i.Activo,
               t.Nombre AS TipoNombre
        FROM Inmuebles i
        JOIN Personas p ON i.PropietarioID = p.ID
        JOIN Tipos_Inmuebles t ON i.TipoId = t.Id
        WHERE i.Activo = 1
          AND NOT EXISTS (
              SELECT 1
              FROM Contratos c
              WHERE c.InmuebleID = i.ID
                AND UPPER(c.Estado) = 'VIGENTE'
                AND (
                     (@inicio BETWEEN c.FechaInicio AND c.FechaFin)
                  OR (@fin BETWEEN c.FechaInicio AND c.FechaFin)
                  OR (c.FechaInicio BETWEEN @inicio AND @fin)
                )
          );";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@inicio", inicio);
            command.Parameters.AddWithValue("@fin", fin);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    TipoNombre = reader.GetString("TipoNombre"), // ✅ cambiado
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo")
                });
            }
            return lista;
        }

        // 🔹 OBTENER POR PROPIETARIO  
        public List<Inmueble> ObtenerPorPropietario(int propietarioId)
        {
            var lista = new List<Inmueble>();
            using var connection = new MySqlConnection(_connectionString);

            const string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio,
               p.Nombre AS NombrePropietario, i.PropietarioID, i.Activo,
               t.Nombre AS TipoNombre,
               i.ImagenUrl
        FROM Inmuebles i
        JOIN Personas p ON i.PropietarioID = p.ID
        JOIN Tipos_Inmuebles t ON i.TipoId = t.Id
        WHERE i.PropietarioID = @propId";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@propId", propietarioId);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                string? rawUrl = reader["ImagenUrl"] == DBNull.Value ? null : reader.GetString("ImagenUrl");

                // 🧩 Corrección robusta: limpia doble /uploads/ y URLs mal formadas
                if (!string.IsNullOrEmpty(rawUrl))
                {
                    // 🔸 Si trae la URL completa, limpia dentro de ella también
                    rawUrl = rawUrl
                        .Replace("http://", "http://") // seguridad por si algún replace global
                        .Replace("https://", "https://") // igual
                        .Replace("/uploads/uploads/", "/uploads/") // ⚙️ fix principal
                        .Replace("//uploads/", "/uploads/");       // doble barra al inicio
                }

                var inmueble = new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    TipoNombre = reader.GetString("TipoNombre"),
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo"),
                    ImagenUrl = rawUrl
                };

                lista.Add(inmueble);
            }

            return lista;
        }



        // 🔹 OBTENER INMUEBLES ALQUILADOS (VIGENTES) POR PROPIETARIO
        public List<Inmueble> ObtenerAlquiladosPorPropietario(int propietarioId)
        {
            var lista = new List<Inmueble>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio,
               p.Nombre AS NombrePropietario, i.PropietarioID, i.Activo,
               t.Nombre AS TipoNombre
        FROM Inmuebles i
        JOIN Personas p ON i.PropietarioID = p.ID
        JOIN Tipos_Inmuebles t ON i.TipoId = t.Id
        JOIN Contratos c ON c.InmuebleID = i.ID
        WHERE i.PropietarioID = @propId
          AND UPPER(c.Estado) = 'VIGENTE'
          AND CURDATE() BETWEEN DATE(c.FechaInicio) AND DATE(c.FechaFin);";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@propId", propietarioId);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    TipoNombre = reader.GetString("TipoNombre"),
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo")
                });
            }

            return lista;
        }


        // 🔹 MODIFICAR (devuelve cantidad de filas afectadas)
        public int Modificacion(Inmueble i)
        {
            using var connection = new MySqlConnection(_connectionString);

            try
            {
                // 🔎 Log detallado para depuración
                Console.WriteLine($"[DEBUG RepoInmueble] UPDATE Id={i.Id}, Dir={i.Direccion}, TipoId={i.TipoId}, PropietarioId={i.PropietarioId}, Precio={i.Precio}, Activo={i.Activo}");

                // ✅ Validaciones antes del UPDATE
                if (i.Id <= 0)
                    throw new Exception("El ID del inmueble no es válido.");
                if (i.TipoId <= 0)
                    throw new Exception("TipoId inválido o no recibido desde el formulario.");
                if (i.PropietarioId <= 0)
                    throw new Exception("PropietarioId inválido o no asignado desde el token.");
                if (i.Precio < 0 || i.Precio > 99999999999.99M)
                    throw new Exception("El precio está fuera del rango permitido.");

                const string sql = @"UPDATE Inmuebles 
                 SET Direccion=@dir, TipoId=@tipoId, MetrosCuadrados=@m2, Precio=@precio, 
                     PropietarioID=@prop, Activo=@activo
                 WHERE ID=@id";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@dir", i.Direccion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@tipoId", i.TipoId);
                command.Parameters.AddWithValue("@m2", i.MetrosCuadrados);
                command.Parameters.AddWithValue("@precio", i.Precio);
                command.Parameters.AddWithValue("@prop", i.PropietarioId);
                command.Parameters.AddWithValue("@activo", i.Activo);
                command.Parameters.AddWithValue("@id", i.Id);

                connection.Open();
                int filas = command.ExecuteNonQuery();

                Console.WriteLine($"[DEBUG RepoInmueble] Filas afectadas: {filas}");
                return filas; // 🔹 devuelve 1 si se actualizó correctamente
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"[ERROR RepoInmueble] Error SQL: {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR RepoInmueble] {ex.Message}");
                throw;
            }
        }

        // 🔹 NUEVO: GUARDAR IMAGEN (actualiza la URL)
        public int GuardarImagen(int idInmueble, string ruta)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "UPDATE Inmuebles SET ImagenUrl = @ruta WHERE ID = @id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ruta", ruta);
            command.Parameters.AddWithValue("@id", idInmueble);

            connection.Open();
            return command.ExecuteNonQuery();
        }
        // 🔹 NUEVO: MODIFICAR CON IMAGEN OPCIONAL (para PUT /form)
        public int ModificacionConImagen(Inmueble i)
        {
            using var connection = new MySqlConnection(_connectionString);

            try
            {
                // 🔎 Depuración: mostrar valores recibidos
                Console.WriteLine($"[DEBUG RepoInmueble] UPDATE Id={i.Id}, Dir={i.Direccion}, TipoId={i.TipoId}, PropietarioId={i.PropietarioId}, Precio={i.Precio}, Activo={i.Activo}, ImagenUrl={i.ImagenUrl}");

                // ✅ Validaciones básicas antes del UPDATE
                if (i.Id <= 0)
                    throw new Exception("El ID del inmueble no es válido.");
                if (i.TipoId <= 0)
                    throw new Exception("TipoId inválido o no recibido desde el formulario.");
                if (i.PropietarioId <= 0)
                    throw new Exception("PropietarioId inválido o no asignado desde el token.");

                // ✅ Validar rango de precio compatible con DECIMAL(12,2)
                if (i.Precio < 0 || i.Precio > 99999999999.99M)
                    throw new Exception("El precio está fuera del rango permitido para la base de datos.");

                // Base de la query
                string sql = @"UPDATE Inmuebles 
                       SET Direccion=@dir, TipoId=@tipoId, MetrosCuadrados=@m2, 
                           Precio=@precio, PropietarioID=@prop, Activo=@activo";

                // Si hay imagen nueva, se incluye la columna
                if (!string.IsNullOrEmpty(i.ImagenUrl))
                    sql += ", ImagenUrl=@img";

                sql += " WHERE ID=@id;";

                using var command = new MySqlCommand(sql, connection);

                // Asignación segura de parámetros
                command.Parameters.AddWithValue("@dir", i.Direccion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@tipoId", i.TipoId);
                command.Parameters.AddWithValue("@m2", i.MetrosCuadrados);
                command.Parameters.AddWithValue("@precio", i.Precio);
                command.Parameters.AddWithValue("@prop", i.PropietarioId);
                command.Parameters.AddWithValue("@activo", i.Activo);
                command.Parameters.AddWithValue("@id", i.Id);

                if (!string.IsNullOrEmpty(i.ImagenUrl))
                    command.Parameters.AddWithValue("@img", i.ImagenUrl);

                connection.Open();
                int filas = command.ExecuteNonQuery();

                Console.WriteLine($"[DEBUG RepoInmueble] Filas afectadas: {filas}");
                return filas; // 🔹 1 si se actualizó correctamente
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"[ERROR RepoInmueble] Error SQL: {sqlEx.Message}");
                throw; // Propaga para que el controlador devuelva 500 con el detalle real
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR RepoInmueble] {ex.Message}");
                throw;
            }
        }


        // 🔹 BAJA LÓGICA
        public void BajaLogica(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "UPDATE Inmuebles SET Activo = 0 WHERE ID = @id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
