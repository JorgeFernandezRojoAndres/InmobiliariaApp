using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public interface IRepoUsuario
    {
        void Crear(Usuario usuario);
        Usuario? ObtenerPorEmail(string email);
        Usuario? ObtenerPorId(int id);

        List<Usuario> ObtenerTodos();
        void Actualizar(Usuario usuario);
        void Eliminar(int id);
    }
}
