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
        private readonly IRepoTipoInmueble _repoTipoInmueble; // 🔹 Nuevo

        // Inyección de dependencias
        public InmueblesController(
            RepoInmueble repoInmueble,
            RepoPersona repoPersona,
            IRepoTipoInmueble repoTipoInmueble) // 🔹 Agregado
        {
            _repoInmueble = repoInmueble;
            _repoPersona = repoPersona;
            _repoTipoInmueble = repoTipoInmueble; // 🔹 Guardamos la ref
        }

        public IActionResult Index()
        {
            var inmuebles = _repoInmueble.Obtener();
            return View(inmuebles);
        }

        // 🔹 Listado de inmuebles disponibles
        public IActionResult Disponibles()
        {
            var lista = _repoInmueble.ObtenerDisponibles();
            ViewData["Title"] = "Inmuebles Disponibles";
            return View("Index", lista);
        }

        // 🔹 Listado de inmuebles por propietario
        public IActionResult PorPropietario(int id)
        {
            var lista = _repoInmueble.ObtenerPorPropietario(id);
            ViewData["Title"] = "Inmuebles del Propietario";
            return View("Index", lista);
        }

        // 🔹 Listado de inmuebles disponibles entre dos fechas
        [HttpGet]
        public IActionResult DisponiblesEntreFechas(DateTime? inicio, DateTime? fin)
        {
            if (!inicio.HasValue || !fin.HasValue)
            {
                ViewData["Title"] = "Buscar inmuebles disponibles por rango de fechas";
                return View(new List<Inmueble>());
            }

            if (inicio > fin)
            {
                ModelState.AddModelError("", "La fecha de inicio no puede ser mayor que la fecha fin.");
                return View(new List<Inmueble>());
            }

            var lista = _repoInmueble.ObtenerDisponiblesEntre(inicio.Value, fin.Value);
            ViewData["Title"] = $"Disponibles entre {inicio:dd/MM/yyyy} y {fin:dd/MM/yyyy}";
            return View(lista);
        }

        public IActionResult Edicion(int id = 0)
        {
            // 🔹 Armar lista con Nombre completo + DNI
            ViewBag.Propietarios = _repoPersona.ObtenerTodos()
                .Select(p => new
                {
                    Id = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellido} - DNI {p.Documento}"
                }).ToList();

            // 🔹 Cargar tipos de inmuebles
            ViewBag.TiposInmuebles = _repoTipoInmueble.ObtenerTodos();

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
            if (inmueble.Id == 0)
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
