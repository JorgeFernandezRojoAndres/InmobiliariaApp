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

        // 🔹 Estado actual del contrato
        public string Estado { get; set; } = "Vigente";

        // 🔹 Auditoría
        public int CreadoPor { get; set; }                  // Id del usuario que lo creó
        public Usuario? UsuarioCreador { get; set; }        // Usuario creador (JOIN con tabla usuarios)
        public int? TerminadoPor { get; set; }              // Id del usuario que lo terminó (nullable)
        public Usuario? UsuarioTerminador { get; set; }     // Usuario terminador (JOIN con tabla usuarios)

        // 🔹 Renovación / Rescisión (NUEVOS CAMPOS)
        public DateTime? FechaRescision { get; set; }       // Fecha en que se rescindió el contrato
        public decimal? MontoMulta { get; set; }            // Multa calculada por rescisión anticipada
        public int? ContratoOriginalId { get; set; }        // Referencia al contrato anterior si es una renovación

        // 🔹 Propiedad calculada para mostrar en dropdowns y vistas
        public string Descripcion
        {
            get
            {
                var inmueble = Inmueble != null ? Inmueble.Direccion : $"Inmueble {IdInmueble}";
                return $"Contrato #{Id} - {inmueble} - {FechaInicio:dd/MM/yyyy} a {FechaFin:dd/MM/yyyy} (${MontoMensual:N2})";
            }
        }
    }
}
