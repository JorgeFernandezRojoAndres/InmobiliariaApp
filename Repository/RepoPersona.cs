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

        // 🔹 Traer todas las personas
        public List<Persona> ObtenerTodos()
        {
            var lista = new List<Persona>();
            using var connection = new MySqlConnection(_connectionString);

            // 🔹 Consulta extendida con GROUP_CONCAT para traer los roles
            const string sql = @"SELECT p.ID, p.Nombre, p.Apellido, p.DNI, p.Email,
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
                    // 🔹 Ahora guardamos los roles en la propiedad Tipo
                    Tipo = reader.IsDBNull(reader.GetOrdinal("Roles"))
                            ? ""
                            : reader.GetString("Roles")
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

        // 🔹 Alta persona (sin rol aún)
        public void Alta(Persona p, List<string> roles)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            int personaId = 0;

            // 1) Verificar si ya existe una persona con ese DNI
            using (var checkCmd = new MySqlCommand("SELECT ID FROM Personas WHERE DNI=@dni", connection))
            {
                checkCmd.Parameters.AddWithValue("@dni", p.Documento);
                var result = checkCmd.ExecuteScalar();
                if (result != null)
                {
                    personaId = Convert.ToInt32(result);
                }
            }

            // 2) Si no existe, insertamos la persona
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

            // 3) Asignar roles (pueden ser varios: Propietario, Inquilino, etc.)
            foreach (var rolNombre in roles)
            {
                int rolId = 0;

                // Buscar el rol en la tabla "roles"
                using (var cmdRol = new MySqlCommand("SELECT id FROM roles WHERE nombre=@nombreRol", connection))
                {
                    cmdRol.Parameters.AddWithValue("@nombreRol", rolNombre);
                    var result = cmdRol.ExecuteScalar();
                    if (result != null)
                    {
                        rolId = Convert.ToInt32(result);
                    }
                }

                // Si existe el rol, insertar en persona_roles (evitando duplicados)
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
        public void Modificar(Persona p)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"UPDATE Personas SET 
                                    Nombre=@nombre, 
                                    Apellido=@apellido, 
                                    DNI=@dni, 
                                    Email=@correo
                                 WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@nombre", p.Nombre);
            command.Parameters.AddWithValue("@apellido", p.Apellido);
            command.Parameters.AddWithValue("@dni", p.Documento);
            command.Parameters.AddWithValue("@correo", p.Email);
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
        // Métodos nuevos para roles
        // =====================================================

        // Agregar un rol a una persona
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

        // Obtener propietarios
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

        // Obtener inquilinos
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
    }
}
