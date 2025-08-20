namespace InmobiliariaApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }   // Usa el enum definido en RolUsuario.cs
        public string PasswordHash { get; set; } = string.Empty;

        // No Required, porque se genera automáticamente
        public string? AvatarUrl { get; set; }
    }
}
