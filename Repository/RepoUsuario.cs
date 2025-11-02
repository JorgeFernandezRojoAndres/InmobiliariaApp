using InmobiliariaApp.Models;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoUsuario : IRepoUsuario
    {
        private readonly string _connectionString;

        // ✅ Ahora también recibe IConfiguration
        public RepoUsuario(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }
        public void Crear(Usuario usuario)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            var sql = "INSERT INTO Usuarios (Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl) " +
                      "VALUES (@Email, @PasswordHash, @Nombre, @Apellido, @Rol, @AvatarUrl)";
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

            cmd.ExecuteNonQuery();
        }

        // ✅ Ahora implementado correctamente
        public Usuario? ObtenerPorEmail(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT Id, Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl " +
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
                        : reader.GetString("AvatarUrl")
                };
            }

            return null;
        }


        public Usuario? ObtenerPorId(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT Id, Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl " +
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
                        : reader.GetString(reader.GetOrdinal("AvatarUrl"))
                };
            }

            return null;
        }


        public List<Usuario> ObtenerTodos()
{
    var lista = new List<Usuario>();
    using var connection = new MySqlConnection(_connectionString);
    connection.Open();

    var sql = "SELECT Id, Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl FROM Usuarios";
    using var cmd = new MySqlCommand(sql, connection);

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
                : reader.GetString(reader.GetOrdinal("AvatarUrl"))
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
