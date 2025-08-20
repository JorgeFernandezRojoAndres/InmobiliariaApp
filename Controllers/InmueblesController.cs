using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly RepoInmueble _repoInmueble;
        private readonly RepoPersona _repoPersona;

        // Inyección de dependencias
        public InmueblesController(RepoInmueble repoInmueble, RepoPersona repoPersona)
        {
            _repoInmueble = repoInmueble;
            _repoPersona = repoPersona;
        }

        public IActionResult Index()
        {
            var inmuebles = _repoInmueble.Obtener();
            return View(inmuebles);
        }

        public IActionResult Edicion(int id = 0)
        {
            ViewBag.Propietarios = _repoPersona.ObtenerTodos();

            if (id == 0)
            {
                return View(new Inmueble());
            }

            var inmueble = _repoInmueble.Obtener(id);

            if (inmueble == null)
            {
                return RedirectToAction("Index");
            }

            return View(inmueble);
        }

        [HttpPost]
        public IActionResult Guardar(Inmueble inmueble)
        {
            if (inmueble.Id
 == 0)
            {
                _repoInmueble.Alta(inmueble);
            }
            else
            {
                _repoInmueble.Modificar(inmueble);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            _repoInmueble.BajaLogica(id);
            TempData["Mensaje"] = "Inmueble eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
