using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Repository;
using InmobiliariaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class InquilinosController : Controller
    {
        private readonly RepoPersona _repo;

        public InquilinosController(RepoPersona repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var lista = _repo.ObtenerInquilinos();
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null || inquilino.Tipo != "Inquilino") return NotFound();
            return View(inquilino);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Persona model)
        {
            if (ModelState.IsValid)
            {
                model.Tipo = "Inquilino"; // 👈 forzamos el tipo
                _repo.Alta(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null || inquilino.Tipo != "Inquilino") return NotFound();
            return View(inquilino);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Persona model)
        {
            if (id != model.Id) return BadRequest();
            if (ModelState.IsValid)
            {
                model.Tipo = "Inquilino"; // 👈 mantenemos tipo
                _repo.Modificar(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null || inquilino.Tipo != "Inquilino") return NotFound();
            return View(inquilino);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino != null && inquilino.Tipo == "Inquilino")
                _repo.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
