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
    }
}
