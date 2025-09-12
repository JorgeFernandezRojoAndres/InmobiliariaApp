using System.Collections.Generic;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public interface IRepoContrato
    {
        IList<Contrato> ObtenerTodos();
        Contrato? ObtenerPorId(int id);
        int Crear(Contrato contrato);
        int Editar(Contrato contrato);
        int Eliminar(int id);
    }
}
