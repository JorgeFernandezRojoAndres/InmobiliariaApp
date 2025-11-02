using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InmobiliariaApp.Controllers
{
    public class TiposInmueblesController : Controller
    {
        private readonly IRepoTipoInmueble _repo;

        public TiposInmueblesController(IRepoTipoInmueble repo)
        {
            _repo = repo;
        }

        // 🏠 Vistas MVC (sin cambios)
        public IActionResult Index()
        {
            var lista = _repo.ObtenerTodos();
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                _repo.Alta(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        public IActionResult Edit(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TipoInmueble tipo)
        {
            if (id != tipo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repo.Modificacion(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        public IActionResult Delete(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Baja(id);
            return RedirectToAction(nameof(Index));
        }
    }

    // ===============================
    // ✅ CONTROLADOR API ASÍNCRONO
    // ===============================
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔐 Requiere JWT válido
    public class TiposInmuebleApiController : ControllerBase
    {
        private readonly IRepoTipoInmueble _repo;

        public TiposInmuebleApiController(IRepoTipoInmueble repo)
        {
            _repo = repo;
        }

        // 🔹 GET: api/TiposInmueble
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoInmueble>>> GetTiposAsync()
        {
            try
            {
                var lista = await _repo.ObtenerTodosAsync();

                if (lista == null || lista.Count == 0)
                    return NotFound("No hay tipos de inmueble registrados.");

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
