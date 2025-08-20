namespace InmobiliariaApp.Models
{
    public class Inmueble
    {
        public int Id { get; set; }   // ⚠️ importante: usar "Id" y no "ID"
        public string Direccion { get; set; } = string.Empty;
        public int MetrosCuadrados { get; set; }
        public decimal Precio { get; set; }
        public int PropietarioId { get; set; }
        public bool Activo { get; set; } = true;
        public string NombrePropietario { get; set; } = string.Empty;
        public string Uso { get; set; } = string.Empty;
    }
}
