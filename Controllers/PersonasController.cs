using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class PersonasController : Controller
    {
        private readonly RepoPersona _repoPersona;

        public PersonasController(RepoPersona repoPersona)
        {
            _repoPersona = repoPersona;
        }

        public IActionResult Index()
        {
            var personas = _repoPersona.ObtenerTodos();
            return View(personas);
        }
    }
}
