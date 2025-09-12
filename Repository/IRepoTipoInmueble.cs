using System.Collections.Generic;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public interface IRepoTipoInmueble
    {
        List<TipoInmueble> ObtenerTodos();
        TipoInmueble? ObtenerPorId(int id);
        int Alta(TipoInmueble tipo);
        int Modificacion(TipoInmueble tipo);
        int Baja(int id);
    }
}
