namespace InmobiliariaApp.Models
{
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty; // mapea DNI
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Inquilino o Propietario
        public string? AvatarUrl { get; set; }

        // 🔹 Nuevas propiedades para autenticación y contacto
        public string? Clave { get; set; }       // Contraseña (encriptada o texto)
        public string? Telefono { get; set; }    // Teléfono del propietario o inquilino

        // 🔑 Campo temporal para recuperación de contraseña
        public string? TokenRecuperacion { get; set; } // Token asociado al enlace de recuperación

        // 🏢 Inmobiliaria (tenant) a la que pertenece la persona
        public int? InmobiliariaId { get; set; }
    }
}
