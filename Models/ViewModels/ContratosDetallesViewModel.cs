using System.Collections.Generic;

namespace InmobiliariaApp.Models.ViewModels
{
    public class ContratoDetallesViewModel
    {
        public Contrato Contrato { get; set; } = new Contrato();
        public IList<Pago> Pagos { get; set; } = new List<Pago>();
    }
}