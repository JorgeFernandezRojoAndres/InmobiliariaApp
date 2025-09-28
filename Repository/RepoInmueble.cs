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
                                 (Direccion, Tipo, MetrosCuadrados, Precio, PropietarioID, Activo)
                                 VALUES (@dir, @tipo, @m2, @precio, @prop, @activo);
                                 SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dir", i.Direccion);
            command.Parameters.AddWithValue("@tipo", i.TipoNombre);  // 🔹 nuevo
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
               t.Nombre AS TipoNombre
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
                lista.Add(new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    TipoNombre = reader.GetString("TipoNombre"), // ✅ corregido
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo")
                });
            }
            return lista;
        }

        // 🔹 MODIFICAR
        public void Modificar(Inmueble i)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"UPDATE Inmuebles 
                         SET Direccion=@dir, TipoId=@tipoId, MetrosCuadrados=@m2, Precio=@precio, 
                             PropietarioID=@prop, Activo=@activo
                         WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dir", i.Direccion);
            command.Parameters.AddWithValue("@tipoId", i.TipoId);          // ✅ usar FK, no el nombre
            command.Parameters.AddWithValue("@m2", i.MetrosCuadrados);
            command.Parameters.AddWithValue("@precio", i.Precio);
            command.Parameters.AddWithValue("@prop", i.PropietarioId);
            command.Parameters.AddWithValue("@activo", i.Activo);
            command.Parameters.AddWithValue("@id", i.Id);

            connection.Open();
            command.ExecuteNonQuery();
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
