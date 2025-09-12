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
        public string? ContratoDescripcion { get; set; }
        
    }
}
