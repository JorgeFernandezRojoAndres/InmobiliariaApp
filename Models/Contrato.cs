using System;

namespace InmobiliariaApp.Models
{
    public class Contrato
    {
        public int Id { get; set; }

        public int IdInquilino { get; set; }
        public Inquilino? Inquilino { get; set; }

        public int IdInmueble { get; set; }
        public Inmueble? Inmueble { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public decimal MontoMensual { get; set; }

        public string? Estado { get; set; }

        // 🔹 Propiedad calculada para mostrar en dropdowns y vistas
        public string Descripcion
        {
            get
            {
                var inmueble = Inmueble != null ? Inmueble.Direccion : $"Inmueble {IdInmueble}";
                return $"Contrato #{Id} - {inmueble} - {FechaInicio:dd/MM/yyyy} a {FechaFin:dd/MM/yyyy} (${MontoMensual})";
            }
        }
    }
}
