namespace InmobiliariaApp.Models
{
    public class Propiedad
    {
        public int Id { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Dormitorios { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
