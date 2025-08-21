using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly RepoPropietario _repo;

        public PropietariosController(RepoPropietario repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var lista = _repo.ObtenerTodos();
            return View(lista);
        }

        // Create, Edit, Details, Delete → igual que en PersonasController
    }
}
