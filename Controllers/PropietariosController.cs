using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies; // ✅ agregado para especificar esquema
using System.Collections.Generic;

namespace InmobiliariaApp.Controllers
{
    // 🔒 Este controlador usa autenticación por COOKIES (no JWT)
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
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
            if (propietario == null)
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
                // 🔹 Alta con rol Propietario
                _repo.Alta(propietario, new List<string> { "Propietario" });

                // 🔹 Redirigir directo al alta de inmueble con el ID del propietario recién creado
                return RedirectToAction("Create", "Inmuebles", new { propietarioId = propietario.Id });
            }
            return View(propietario);
        }

        // GET: /Propietarios/Edit/5
        public IActionResult Edit(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null)
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
                _repo.Modificar(propietario);
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: /Propietarios/Delete/5
        public IActionResult Delete(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null)
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
