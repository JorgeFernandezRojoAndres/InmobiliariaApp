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
        int RenovarContrato(int idContratoOriginal, DateTime nuevaFechaInicio, DateTime nuevaFechaFin, decimal nuevoMonto, int idPropietario);

        int Crear(Contrato contrato);
        int Editar(Contrato contrato);
        int Eliminar(int idContrato, int idUsuario);

        // 🔹 Para filtros de fechas
        IList<Contrato> ObtenerVigentesEntre(DateTime inicio, DateTime fin);

        // 🔹 Ya lo usás en vencimiento automático
        int MarcarComoVencido(int id);

        // ✅ Ya existentes para la app móvil
        IList<Pago> ObtenerPagosPorContrato(int contratoId);
        IList<Contrato> ObtenerVigentesPorPropietario(int idPropietario);

        // ✅ Nuevos métodos para filtros móviles
        IList<Contrato> ObtenerFinalizadosPorPropietario(int idPropietario);
        IList<Contrato> ObtenerTodosPorPropietario(int idPropietario);

        // ✅ NUEVO para evitar renovar si hay otro contrato vigente
        bool ExisteContratoVigenteParaInmueble(int idInmueble, int idContratoActual);
    }
}
