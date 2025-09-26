namespace InmobiliariaApp.Models
{
    public class Inmueble
    {
        public int Id { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; 
        public int MetrosCuadrados { get; set; }
        public decimal Precio { get; set; }
        public int PropietarioId { get; set; }
        public bool Activo { get; set; } = true;

        // Prop extra para mostrar nombre del propietario
        public string NombrePropietario { get; set; } = string.Empty;

        // 🔹 Prop calculada para combos/dropdowns
        public string Descripcion =>
            $"{Direccion} - {Tipo} (${Precio}) - {NombrePropietario}";
    }
}
