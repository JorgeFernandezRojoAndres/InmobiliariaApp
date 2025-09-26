using System;
using System.Collections.Generic;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public interface IRepoContrato
    {
        IList<Contrato> ObtenerTodos();
        Contrato? ObtenerPorId(int id);
        IList<Contrato> ObtenerPorInmueble(int inmuebleId);

        int Crear(Contrato contrato);
        int Editar(Contrato contrato);
        int Eliminar(int idContrato, int idUsuario);


        // 🔹 Nuevo método para promoción
        IList<Contrato> ObtenerVigentesEntre(DateTime inicio, DateTime fin);

        // 🔹 Ya lo usás en RepoContrato (cuando marcás contratos vencidos)
        int MarcarComoVencido(int id);
    }
}
