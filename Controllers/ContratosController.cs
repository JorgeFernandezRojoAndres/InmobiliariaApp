using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Controllers
{
    public class ContratosController : Controller
    {
        private readonly IRepoContrato repo;
        private readonly RepoPersona repoPersona;
        private readonly RepoInmueble repoInmueble;

        public ContratosController(IRepoContrato repo, RepoPersona repoPersona, RepoInmueble repoInmueble)
        {
            this.repo = repo;
            this.repoPersona = repoPersona;
            this.repoInmueble = repoInmueble;
        }

        public IActionResult Index()
        {
            var contratos = repo.ObtenerTodos();
            return View(contratos);
        }

        public IActionResult Details(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        // GET: Contratos/Create
        public IActionResult Create()
        {
            CargarSelects();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    repo.Crear(contrato);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    // 🚨 Validación de lógica propia en RepoContrato
                    ModelState.AddModelError("", ex.Message);
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    // 🚨 Error levantado por el trigger
                    if (ex.Message.Contains("❌"))
                        ModelState.AddModelError("", ex.Message);
                    else
                        ModelState.AddModelError("", "⚠️ Error inesperado al guardar el contrato.");
                }
            }

            // recargar selects
            CargarSelects(contrato.IdInquilino, contrato.IdInmueble);
            return View(contrato);
        }


        public IActionResult Edit(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();

            CargarSelects(contrato.IdInquilino, contrato.IdInmueble);
            return View(contrato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Contrato contrato)
        {
            if (id != contrato.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    repo.Editar(contrato);
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            CargarSelects(contrato.IdInquilino, contrato.IdInmueble);
            return View(contrato);
        }

        public IActionResult Delete(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();

            // 🔹 Si ya venció -> marcar como vencido
            if (contrato.FechaFin <= DateTime.Now)
            {
                contrato.Estado = "Vencido";
                repo.Editar(contrato); // Solo actualizamos estado
            }
            else
            {
                // 🔹 Si aún está vigente -> se elimina realmente
                repo.Eliminar(id);
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔹 Método privado para no repetir código
        private void CargarSelects(int? inquilinoId = null, int? inmuebleId = null)
        {
            var inquilinos = repoPersona.ObtenerInquilinos()
                .Select(i => new
                {
                    Id = i.Id,
                    Display = $"{i.Nombre} {i.Apellido} - DNI {i.Documento}"
                }).ToList();

            var inmuebles = repoInmueble.Obtener()
                .Select(im => new
                {
                    Id = im.Id,
                    Display = $"{im.Direccion} - {im.Tipo} (${im.Precio})"
                }).ToList();

            ViewBag.Inquilinos = new SelectList(inquilinos, "Id", "Display", inquilinoId);
            ViewBag.Inmuebles = new SelectList(inmuebles, "Id", "Display", inmuebleId);
        }
    }
}
