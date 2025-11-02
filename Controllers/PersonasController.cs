using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

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

        // GET: /Personas
        public IActionResult Index()
        {
            var personas = _repoPersona.ObtenerTodos();
            return View(personas);
        }

        // GET: /Personas/Details/5
        public IActionResult Details(int id)
        {
            var persona = _repoPersona.ObtenerPorId(id);
            if (persona == null) return NotFound();
            return View(persona);
        }

        // GET: /Personas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Personas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Persona persona, bool esPropietario, bool esInquilino)
        {
            if (ModelState.IsValid)
            {
                var roles = new List<string>();
                if (esPropietario) roles.Add("Propietario");
                if (esInquilino) roles.Add("Inquilino");

                // Si no se marca ningún rol, se da de alta sin roles (queda pendiente de asignar luego)
                _repoPersona.Alta(persona, roles);

                return RedirectToAction(nameof(Index));
            }
            return View(persona);
        }

        // GET: /Personas/Edit/5
        public IActionResult Edit(int id)
        {
            var persona = _repoPersona.ObtenerPorId(id);
            if (persona == null) return NotFound();
            return View(persona);
        }

        // POST: /Personas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Persona persona)
        {
            if (id != persona.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                _repoPersona.Modificar(persona);
                return RedirectToAction(nameof(Index));
            }
            return View(persona);
        }

        // GET: /Personas/Delete/5
        public IActionResult Delete(int id)
        {
            var persona = _repoPersona.ObtenerPorId(id);
            if (persona == null) return NotFound();
            return View(persona);
        }

        // POST: /Personas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repoPersona.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
