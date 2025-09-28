using System.Collections.Generic;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public interface IRepoTipoInmueble
    {
        // 🔹 Traer todos los tipos de inmueble
        List<TipoInmueble> ObtenerTodos();

        // 🔹 Buscar un tipo por su Id
        TipoInmueble? ObtenerPorId(int id);

        // 🔹 Alta de un nuevo tipo
        int Alta(TipoInmueble tipo);

        // 🔹 Modificación de un tipo existente
        int Modificacion(TipoInmueble tipo);

        // 🔹 Baja lógica o física según lo definas en RepoTipoInmueble
        int Baja(int id);
    }
}
