using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Controllers
{
    public class TiposInmueblesController : Controller
    {
        private readonly IRepoTipoInmueble _repo;

        // ✅ Ahora el repo se inyecta, no se instancia manualmente
        public TiposInmueblesController(IRepoTipoInmueble repo)
        {
            _repo = repo;
        }

        // GET: /TiposInmuebles
        public IActionResult Index()
        {
            var lista = _repo.ObtenerTodos();
            return View(lista);
        }

        // GET: /TiposInmuebles/Details/5
        public IActionResult Details(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        // GET: /TiposInmuebles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /TiposInmuebles/Create
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

        // GET: /TiposInmuebles/Edit/5
        public IActionResult Edit(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        // POST: /TiposInmuebles/Edit/5
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

        // GET: /TiposInmuebles/Delete/5
        public IActionResult Delete(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        // POST: /TiposInmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Baja(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
