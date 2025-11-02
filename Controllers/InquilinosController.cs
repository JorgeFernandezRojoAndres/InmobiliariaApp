using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Repository;
using InmobiliariaApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

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

        // GET: /Inquilinos
        public IActionResult Index()
        {
            var lista = _repo.ObtenerInquilinos();
            return View(lista);
        }

        // GET: /Inquilinos/Details/5
        public IActionResult Details(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null)
                return NotFound();

            return View(inquilino);
        }

        // GET: /Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Persona model)
        {
            if (ModelState.IsValid)
            {
                // 🔹 Alta con rol Inquilino
                _repo.Alta(model, new List<string> { "Inquilino" });
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Inquilinos/Edit/5
        public IActionResult Edit(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null)
                return NotFound();

            return View(inquilino);
        }

        // POST: /Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Persona model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                _repo.Modificar(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Inquilinos/Delete/5
        public IActionResult Delete(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null)
                return NotFound();

            return View(inquilino);
        }

        // POST: /Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino != null)
            {
                _repo.Eliminar(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
