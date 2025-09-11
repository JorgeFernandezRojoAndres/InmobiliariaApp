using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Data
{
    public static class DbSeeder
    {
        public static void Seed(IRepoUsuario repoUsuario)
        {
            // Lista de usuarios iniciales
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    Email = "admin@inmobiliaria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"), // 👈 contraseña: 1234
                    Nombre = "Admin",
                    Apellido = "Principal",
                    Rol = RolUsuario.Administrador,
                    AvatarUrl = "/avatars/default.png"
                },
                new Usuario
                {
                    Email = "empleado@inmobiliaria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"), // 👈 contraseña: 1234
                    Nombre = "Empleado",
                    Apellido = "Ejemplo",
                    Rol = RolUsuario.Empleado,
                    AvatarUrl = "/avatars/default.png"
                }
            };

            foreach (var usuario in usuarios)
            {
                var existente = repoUsuario.ObtenerPorEmail(usuario.Email);
                if (existente == null)
                {
                    repoUsuario.Crear(usuario);
                    Console.WriteLine($"✅ Usuario creado: {usuario.Email} ({usuario.Rol})");
                }
                else
                {
                    Console.WriteLine($"ℹ️ Usuario ya existe: {usuario.Email}");
                }
            }
        }
    }
}
