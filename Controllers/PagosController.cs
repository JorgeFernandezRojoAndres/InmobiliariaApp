using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Controllers
{
    public class PagosController : Controller
    {
        private readonly IRepoPago repo;
        private readonly RepoContrato repoContratos;

        public PagosController()
        {
            repo = new RepoPago();
            repoContratos = new RepoContrato();
        }

        // GET: /Pagos
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        // GET: /Pagos/Details/5
        public IActionResult Details(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // GET: /Pagos/Create
        public IActionResult Create()
        {
            ViewBag.Contratos = repoContratos.ObtenerTodos();
            return View();
        }

        // POST: /Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pago pago)
        {
            if (ModelState.IsValid)
            {
                repo.Alta(pago);
                return RedirectToAction(nameof(Index));
            }

            // 🔹 Si falla la validación, volvemos a cargar contratos
            ViewBag.Contratos = repoContratos.ObtenerTodos();
            return View(pago);
        }

        // GET: /Pagos/Edit/5
        public IActionResult Edit(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();

            ViewBag.Contratos = repoContratos.ObtenerTodos();
            return View(pago);
        }

        // POST: /Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Pago pago)
        {
            if (id != pago.Id) return NotFound();
            if (ModelState.IsValid)
            {
                repo.Modificacion(pago);
                return RedirectToAction(nameof(Index));
            }

            // 🔹 Si hay error de validación, recargamos contratos
            ViewBag.Contratos = repoContratos.ObtenerTodos();
            return View(pago);
        }

        // GET: /Pagos/Delete/5
        public IActionResult Delete(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // POST: /Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.Baja(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
