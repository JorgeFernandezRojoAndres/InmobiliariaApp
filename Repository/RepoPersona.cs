using MySqlConnector;
using System.Collections.Generic;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoPersona
    {
        private readonly string _connectionString;

        public RepoPersona(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        // 🔹 Traer todas las personas con roles concatenados
        public List<Persona> ObtenerTodos()
        {
            var lista = new List<Persona>();
            using var connection = new MySqlConnection(_connectionString);

            const string sql = @"
                SELECT p.ID, p.Nombre, p.Apellido, p.DNI, p.Email,
                       GROUP_CONCAT(r.nombre SEPARATOR ', ') AS Roles
                FROM Personas p
                LEFT JOIN persona_roles pr ON p.ID = pr.persona_id
                LEFT JOIN roles r ON pr.rol_id = r.id
                GROUP BY p.ID, p.Nombre, p.Apellido, p.DNI, p.Email";

            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Persona
                {
                    Id = reader.GetInt32("ID"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email"),
                    // ⚠️ Eliminamos "Tipo" — ahora usamos Roles concatenados si existen
                    Clave = "",
                    Telefono = "",
                    AvatarUrl = null
                });
            }
            return lista;
        }

        // 🔹 Obtener una persona específica
        public Persona? ObtenerPorId(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "SELECT ID, Nombre, Apellido, DNI, Email FROM Personas WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Persona
                {
                    Id = reader.GetInt32("ID"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email")
                };
            }
            return null;
        }

        // 🔹 Alta persona + asignar roles
        public void Alta(Persona p, List<string> roles)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            int personaId = 0;

            // 1️⃣ Verificar si ya existe una persona con ese DNI
            using (var checkCmd = new MySqlCommand("SELECT ID FROM Personas WHERE DNI=@dni", connection))
            {
                checkCmd.Parameters.AddWithValue("@dni", p.Documento);
                var result = checkCmd.ExecuteScalar();
                if (result != null)
                    personaId = Convert.ToInt32(result);
            }

            // 2️⃣ Insertar si no existe
            if (personaId == 0)
            {
                const string sql = @"INSERT INTO Personas (Nombre, Apellido, DNI, Email) 
                                     VALUES (@nombre, @apellido, @dni, @correo)";
                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@nombre", p.Nombre);
                command.Parameters.AddWithValue("@apellido", p.Apellido);
                command.Parameters.AddWithValue("@dni", p.Documento);
                command.Parameters.AddWithValue("@correo", p.Email);
                command.ExecuteNonQuery();
                personaId = (int)command.LastInsertedId;
            }

            p.Id = personaId;

            // 3️⃣ Asignar roles existentes
            foreach (var rolNombre in roles)
            {
                int rolId = 0;
                using (var cmdRol = new MySqlCommand("SELECT id FROM roles WHERE nombre=@nombreRol", connection))
                {
                    cmdRol.Parameters.AddWithValue("@nombreRol", rolNombre);
                    var result = cmdRol.ExecuteScalar();
                    if (result != null)
                        rolId = Convert.ToInt32(result);
                }

                if (rolId > 0)
                {
                    using var cmdPersonaRol = new MySqlCommand(@"
                        INSERT IGNORE INTO persona_roles (persona_id, rol_id) 
                        VALUES (@personaId, @rolId)", connection);
                    cmdPersonaRol.Parameters.AddWithValue("@personaId", p.Id);
                    cmdPersonaRol.Parameters.AddWithValue("@rolId", rolId);
                    cmdPersonaRol.ExecuteNonQuery();
                }
            }
        }

        // 🔹 Modificar persona
        // 🔹 Modificar persona (corrige persistencia del AvatarUrl)
        public void Modificar(Persona p)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"UPDATE Personas SET 
                            Nombre=@nombre, 
                            Apellido=@apellido, 
                            DNI=@dni, 
                            Email=@correo,
                            Telefono=@telefono,
                            AvatarUrl=@avatar,
                            Clave=@clave
                         WHERE ID=@id";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@nombre", p.Nombre);
            command.Parameters.AddWithValue("@apellido", p.Apellido);
            command.Parameters.AddWithValue("@dni", p.Documento);
            command.Parameters.AddWithValue("@correo", p.Email);
            command.Parameters.AddWithValue("@telefono", p.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@avatar", p.AvatarUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@clave", p.Clave ?? (object)DBNull.Value); // ✅ agregado
            command.Parameters.AddWithValue("@id", p.Id);

            connection.Open();
            command.ExecuteNonQuery();
        }


        // 🔹 Eliminar persona
        public void Eliminar(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "DELETE FROM Personas WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }

        // =====================================================
        // 🔹 Métodos para roles y autenticación
        // =====================================================

        public void AsignarRol(int personaId, int rolId)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"INSERT INTO persona_roles (persona_id, rol_id)
                                 VALUES (@personaId, @rolId)";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@personaId", personaId);
            command.Parameters.AddWithValue("@rolId", rolId);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public List<Persona> ObtenerPropietarios()
        {
            var lista = new List<Persona>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"SELECT p.ID, p.Nombre, p.Apellido, p.DNI, p.Email
                                 FROM Personas p
                                 JOIN persona_roles pr ON p.ID = pr.persona_id
                                 JOIN roles r ON pr.rol_id = r.id
                                 WHERE r.nombre = 'Propietario'";
            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Persona
                {
                    Id = reader.GetInt32("ID"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email")
                });
            }
            return lista;
        }

        public List<Persona> ObtenerInquilinos()
        {
            var lista = new List<Persona>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"SELECT p.ID, p.Nombre, p.Apellido, p.DNI, p.Email
                                 FROM Personas p
                                 JOIN persona_roles pr ON p.ID = pr.persona_id
                                 JOIN roles r ON pr.rol_id = r.id
                                 WHERE r.nombre = 'Inquilino'";
            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Persona
                {
                    Id = reader.GetInt32("ID"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email")
                });
            }
            return lista;
        }

        // ✅ Obtener persona por email (login móvil)
        public Persona? ObtenerPorEmail(string email)
        {
            Persona? persona = null;
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "SELECT * FROM personas WHERE Email = @Email";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            connection.Open();
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                persona = new Persona
                {
                    Id = reader.GetInt32("Id"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email"),
                    Clave = reader["Clave"]?.ToString(),
                    Telefono = reader["Telefono"]?.ToString(),
                    // ✅ Verifica que exista la columna antes de intentar leerla
                    AvatarUrl = MySqlReaderExtensions.HasColumn(reader, "AvatarUrl") ? reader["AvatarUrl"]?.ToString() : null
                };
            }
            return persona;
        }

        // =====================================================
        // ✅ Helper de extensión (definilo DENTRO del namespace, FUERA de la clase RepoPersona)
        // =====================================================
        internal static class MySqlReaderExtensions
        {
            public static bool HasColumn(MySqlDataReader reader, string columnName)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
        }


        // ✅ Verificar si tiene un rol determinado
        public bool TieneRol(int personaId, string nombreRol)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"
                SELECT COUNT(*) 
                FROM persona_roles pr 
                INNER JOIN roles r ON pr.rol_id = r.id 
                WHERE pr.persona_id = @Id AND r.nombre = @Rol";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", personaId);
            command.Parameters.AddWithValue("@Rol", nombreRol);
            connection.Open();
            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        // ✅ Nuevo: Obtener lista de roles por persona
        public List<string> ObtenerRoles(int personaId)
        {
            var roles = new List<string>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"
                SELECT r.nombre 
                FROM persona_roles pr
                INNER JOIN roles r ON pr.rol_id = r.id
                WHERE pr.persona_id = @Id";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", personaId);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                roles.Add(reader.GetString("nombre"));
            }
            return roles;
        }
        public List<dynamic> ObtenerInquilinosConInmueble(int propietarioId)
        {
            var lista = new List<dynamic>();

            using var connection = new MySqlConnection(_connectionString);

            const string sql = @"
        SELECT 
            p.ID AS InquilinoId,
            p.Nombre,
            p.Apellido,
            p.DNI,
            p.Email,
            i.ID AS InmuebleId,
            i.Direccion,
            i.ImagenUrl
        FROM personas p
        JOIN persona_roles pr ON p.ID = pr.persona_id
        JOIN roles r ON pr.rol_id = r.id AND r.nombre = 'Inquilino'
        JOIN contratos c ON c.InquilinoID = p.ID AND c.Estado = 'Vigente'
        JOIN inmuebles i ON i.ID = c.InmuebleID
        WHERE i.PropietarioID = @propietarioId;
    ";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@propietarioId", propietarioId);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new
                {
                    InquilinoId = reader.GetInt32("InquilinoId"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    DNI = reader.GetString("DNI"),
                    Email = reader.GetString("Email"),
                    Inmueble = new
                    {
                        Id = reader.GetInt32("InmuebleId"),
                        Direccion = reader.GetString("Direccion"),
                        ImagenUrl = reader["ImagenUrl"] != DBNull.Value
                            ? reader.GetString("ImagenUrl")
                            : null
                    }
                });
            }

            return lista;
        }
        public dynamic? ObtenerInquilinoPorId(int idInquilino)
        {
            using var conn = new MySqlConnection(_connectionString);

            const string sql = @"
        SELECT 
            p.ID,
            p.Nombre,
            p.Apellido,
            p.DNI,
            p.Email,
            p.Telefono,
            inm.ID AS InmuebleId,
            inm.Direccion,
            inm.ImagenUrl AS ImagenUrlInmueble
        FROM personas p
        JOIN persona_roles pr ON p.ID = pr.persona_id
        JOIN roles r ON pr.rol_id = r.id AND r.nombre = 'Inquilino'
        JOIN contratos c ON c.InquilinoID = p.ID
        JOIN inmuebles inm ON inm.ID = c.InmuebleID
        WHERE p.ID = @id
        LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idInquilino);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new
                {
                    Id = reader.GetInt32("ID"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Dni = reader.GetString("DNI"),
                    Email = reader.GetString("Email"),
                    Telefono = reader["Telefono"] != DBNull.Value ? reader.GetString("Telefono") : "-",
                    Direccion = reader.GetString("Direccion"),
                    ImagenUrlInmueble = reader["ImagenUrlInmueble"] != DBNull.Value
                                        ? reader.GetString("ImagenUrlInmueble")
                                        : null
                };
            }

            return null;
        }


    }
}
