using InmobiliariaApp.Models;
using InmobiliariaApp.Helpers;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoUsuario : IRepoUsuario
    {
        private readonly string _connectionString;
        private readonly IInmobiliariaContext? _inmobiliaria;

        // ✅ Ahora también recibe IConfiguration
        public RepoUsuario(IConfiguration configuration, IInmobiliariaContext? inmobiliaria = null)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
            _inmobiliaria = inmobiliaria;
        }

        // 🔒 Filtro de aislamiento: null = sin filtro (API móvil / sin sesión web)
        private void AgregarFiltroInmobiliaria(MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@inmobiliariaId",
                (object?)_inmobiliaria?.InmobiliariaId ?? DBNull.Value);
        }

        public void Crear(Usuario usuario)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            var sql = "INSERT INTO Usuarios (Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl, InmobiliariaId) " +
                      "VALUES (@Email, @PasswordHash, @Nombre, @Apellido, @Rol, @AvatarUrl, @InmobiliariaId)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
            cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
            cmd.Parameters.AddWithValue("@Rol", usuario.Rol.ToString());
            var avatarValue = string.IsNullOrWhiteSpace(usuario.AvatarUrl)
                ? "/avatars/default.png"
                : usuario.AvatarUrl;

            cmd.Parameters.AddWithValue("@AvatarUrl", avatarValue);
            cmd.Parameters.AddWithValue("@InmobiliariaId",
                (object?)usuario.InmobiliariaId ?? DBNull.Value);

            cmd.ExecuteNonQuery();
            var nuevoId = (int)cmd.LastInsertedId;
            usuario.Id = nuevoId;

            // ✅ Si no se indicó inmobiliaria, el usuario es el dueño de la suya propia.
            if (usuario.InmobiliariaId == null)
            {
                using var cmdUpdate = new MySqlCommand(
                    "UPDATE Usuarios SET InmobiliariaId = Id WHERE Id = @Id AND InmobiliariaId IS NULL", connection);
                cmdUpdate.Parameters.AddWithValue("@Id", nuevoId);
                cmdUpdate.ExecuteNonQuery();
                usuario.InmobiliariaId = nuevoId;
            }
        }

        // ✅ Ahora implementado correctamente
        public Usuario? ObtenerPorEmail(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // ⚠️ Sin filtro por inmobiliaria a propósito: el login debe poder encontrar
            //    al usuario sin importar de qué inmobiliaria sea.
            var sql = "SELECT Id, Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl, InmobiliariaId " +
                      "FROM Usuarios WHERE LOWER(Email) = LOWER(@Email) LIMIT 1";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Usuario
                {
                    Id = reader.GetInt32("Id"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Rol = Enum.TryParse<RolUsuario>(reader.GetString("Rol"), true, out var rol) ? rol : RolUsuario.Empleado,
                    AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl"))
                        ? "/avatars/default.png"
                        : reader.GetString("AvatarUrl"),
                    InmobiliariaId = reader.IsDBNull(reader.GetOrdinal("InmobiliariaId"))
                        ? (int?)null
                        : reader.GetInt32("InmobiliariaId")
                };
            }

            return null;
        }


        public Usuario? ObtenerPorId(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // ⚠️ Sin filtro por inmobiliaria a propósito: lo usa InmobiliariaContext para
            //    resolver la inmobiliaria del usuario (evitaría recursión) y el login.
            var sql = "SELECT Id, Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl, InmobiliariaId " +
                      "FROM Usuarios WHERE Id = @Id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Usuario
                {
                    Id = reader.GetInt32("Id"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Rol = Enum.Parse<RolUsuario>(reader.GetString("Rol")),
                    AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl"))
                        ? "/avatars/default.png"   // ✅ usa default si está en NULL
                        : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                    InmobiliariaId = reader.IsDBNull(reader.GetOrdinal("InmobiliariaId"))
                        ? (int?)null
                        : reader.GetInt32("InmobiliariaId")
                };
            }

            return null;
        }


        public List<Usuario> ObtenerTodos()
{
    var lista = new List<Usuario>();
    using var connection = new MySqlConnection(_connectionString);
    connection.Open();

    // 🔒 Solo los usuarios de la inmobiliaria del que pide la lista
    var sql = "SELECT Id, Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl, InmobiliariaId " +
              "FROM Usuarios WHERE @inmobiliariaId IS NULL OR InmobiliariaId = @inmobiliariaId";
    using var cmd = new MySqlCommand(sql, connection);
    AgregarFiltroInmobiliaria(cmd);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        lista.Add(new Usuario
        {
            Id = reader.GetInt32("Id"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash"),
            Nombre = reader.GetString("Nombre"),
            Apellido = reader.GetString("Apellido"),
            Rol = Enum.Parse<RolUsuario>(reader.GetString("Rol")),
            AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl"))
                ? "/avatars/default.png"   // ✅ mismo fix que en ObtenerPorId
                : reader.GetString(reader.GetOrdinal("AvatarUrl")),
            InmobiliariaId = reader.IsDBNull(reader.GetOrdinal("InmobiliariaId"))
                ? (int?)null
                : reader.GetInt32("InmobiliariaId")
        });
    }

    return lista;
}


        public void Actualizar(Usuario usuario)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = "UPDATE Usuarios SET Email=@Email, PasswordHash=@PasswordHash, " +
                      "Nombre=@Nombre, Apellido=@Apellido, Rol=@Rol, AvatarUrl=@AvatarUrl " +
                      "WHERE Id=@Id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", usuario.Id);
            cmd.Parameters.AddWithValue("@Email", usuario.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
            cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
            cmd.Parameters.AddWithValue("@Rol", usuario.Rol.ToString());
            var avatarValue = string.IsNullOrWhiteSpace(usuario.AvatarUrl)
                ? (object)"/avatars/default.png"
                : usuario.AvatarUrl;

            cmd.Parameters.AddWithValue("@AvatarUrl", avatarValue);

            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = "DELETE FROM Usuarios WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
