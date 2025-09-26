using InmobiliariaApp.Models;
using System.Collections.Generic;

namespace InmobiliariaApp.Repository
{
    public interface IRepoPago
    {
        List<Pago> ObtenerTodos();
        List<Pago> ObtenerPorContrato(int contratoId);
        Pago? ObtenerPorId(int id);
        int Alta(Pago pago);
        int Modificacion(Pago pago);
        int Baja(int id, int anuladoPorId);

    }
}
