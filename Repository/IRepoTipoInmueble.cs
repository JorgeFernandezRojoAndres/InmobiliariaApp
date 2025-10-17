using System.Collections.Generic;
using System.Threading.Tasks;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public interface IRepoTipoInmueble
    {
        // ===============================
        // 🔹 Métodos síncronos (compatibles con MVC)
        // ===============================
        List<TipoInmueble> ObtenerTodos();
        TipoInmueble? ObtenerPorId(int id);
        int Alta(TipoInmueble tipo);
        int Modificacion(TipoInmueble tipo);
        int Baja(int id);

        // ===============================
        // 🔹 NUEVOS métodos asíncronos (para API y rendimiento)
        // ===============================
        Task<List<TipoInmueble>> ObtenerTodosAsync();
        Task<TipoInmueble?> ObtenerPorIdAsync(int id);
        Task<int> AltaAsync(TipoInmueble tipo);
        Task<int> ModificacionAsync(TipoInmueble tipo);
        Task<int> BajaAsync(int id);
    }
}
