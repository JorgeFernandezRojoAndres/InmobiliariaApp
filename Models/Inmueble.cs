namespace InmobiliariaApp.Models
{
    public class Inmueble
    {
        public int Id { get; set; }
        public string Direccion { get; set; } = string.Empty;

        // 🔹 FK hacia la tabla tipos_inmuebles
        public int TipoId { get; set; }

        // 🔹 Nombre del tipo (JOIN con tipos_inmuebles)
        public string TipoNombre { get; set; } = string.Empty;

        public int MetrosCuadrados { get; set; }
        public decimal Precio { get; set; }
        public int PropietarioId { get; set; }
        public bool Activo { get; set; } = true;
        // 🖼️ URL o ruta de imagen del inmueble (nullable)
        public string? ImagenUrl { get; set; }

        // 🔹 Nombre del propietario (JOIN con propietarios)
        public string NombrePropietario { get; set; } = string.Empty;

        // 🔹 Prop calculada para mostrar en combos/dropdowns
        public string Descripcion =>
    $"{Direccion} - {TipoNombre} (${Precio.ToString(System.Globalization.CultureInfo.InvariantCulture)}) - {NombrePropietario}";

    }
}
