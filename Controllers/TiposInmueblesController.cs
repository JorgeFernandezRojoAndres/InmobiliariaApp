using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Controllers
{
    public class TiposInmueblesController : Controller
    {
        private readonly IRepoTipoInmueble repo;

        public TiposInmueblesController()
        {
            // ⚠️ Ajustá el repo si usás inyección de dependencias
            repo = new RepoTipoInmueble();
        }

        // GET: /TiposInmuebles
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        // GET: /TiposInmuebles/Details/5
        public IActionResult Details(int id)
        {
            var tipo = repo.ObtenerPorId(id);
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
                repo.Alta(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // GET: /TiposInmuebles/Edit/5
        public IActionResult Edit(int id)
        {
            var tipo = repo.ObtenerPorId(id);
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
                repo.Modificacion(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // GET: /TiposInmuebles/Delete/5
        public IActionResult Delete(int id)
        {
            var tipo = repo.ObtenerPorId(id);
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
            repo.Baja(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
