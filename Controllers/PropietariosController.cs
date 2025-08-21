using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly RepoPersona _repo;

        public PropietariosController(RepoPersona repo)
        {
            _repo = repo;
        }

        // GET: /Propietarios
        public IActionResult Index()
        {
            var lista = _repo.ObtenerPropietarios();
            return View(lista);
        }

        // GET: /Propietarios/Details/5
        public IActionResult Details(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null || propietario.Tipo != "Propietario")
                return NotFound();

            return View(propietario);
        }

        // GET: /Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Persona propietario)
        {
            if (ModelState.IsValid)
            {
                propietario.Tipo = "Propietario"; // 🔹 se fuerza el tipo
                _repo.Alta(propietario);
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: /Propietarios/Edit/5
        public IActionResult Edit(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null || propietario.Tipo != "Propietario")
                return NotFound();

            return View(propietario);
        }

        // POST: /Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Persona propietario)
        {
            if (id != propietario.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                propietario.Tipo = "Propietario"; // 🔹 se asegura que no cambie
                _repo.Modificar(propietario);
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: /Propietarios/Delete/5
        public IActionResult Delete(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null || propietario.Tipo != "Propietario")
                return NotFound();

            return View(propietario);
        }

        // POST: /Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
