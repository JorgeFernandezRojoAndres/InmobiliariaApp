using System;

namespace InmobiliariaApp.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int ContratoId { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Detalle { get; set; }
        public decimal Importe { get; set; }

        // Opcional: para mostrar en las vistas sin Join
        public string? ContratoDescripcion { get; set; }

        // 🔹 Auditoría
        public int CreadoPorId { get; set; }           // FK al Usuario que lo creó
        public Usuario? CreadoPor { get; set; }        // Navegación

        public int? AnuladoPorId { get; set; }         // FK al Usuario que lo anuló
        public Usuario? AnuladoPor { get; set; }
    }
}
