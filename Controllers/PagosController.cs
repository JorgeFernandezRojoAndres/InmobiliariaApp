using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FirebaseAdmin.Messaging; // 🆕 Enviar notificaciones FCM
using System.Threading.Tasks;

namespace InmobiliariaApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IRepoPago repo;
        private readonly IRepoContrato repoContratos;

        // 🔹 Constructor con inyección de dependencias
        public PagosController(IRepoPago repo, IRepoContrato repoContratos)
        {
            this.repo = repo;
            this.repoContratos = repoContratos;
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
        public async Task<IActionResult> Create(Pago pago)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 👤 Obtener ID del usuario logueado desde los Claims
                    var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null)
                    {
                        int idUsuario = int.Parse(claim.Value);
                        pago.CreadoPor = idUsuario;
                    }

                    // 💾 Guardar en base de datos
                    repo.Alta(pago);
                    TempData["SuccessMessage"] = "✅ Pago registrado correctamente.";

                    // 🔔 Enviar notificación Firebase
                    await EnviarNotificacionPago(pago);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al registrar el pago: {ex.Message}");
                }
            }

            // 🔹 Si falla la validación o hay error, recargamos contratos
            ViewBag.Contratos = repoContratos.ObtenerTodos();
            return View(pago);
        }

        // 🆕 🔔 Método para enviar notificación FCM con datos adicionales
        private async Task EnviarNotificacionPago(Pago pago)
        {
            try
            {
                string titulo = "Nuevo pago registrado";
                string cuerpo = $"Se acreditó el pago #{pago.Id} del contrato #{pago.ContratoId}.";

                var message = new Message()
                {
                    Notification = new Notification
                    {
                        Title = titulo,
                        Body = cuerpo
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "title", titulo },
                        { "body", $"Pago #{pago.Id} confirmado correctamente." },
                        { "screen", "pagos" }
                    },
                    // 🔹 Todos los dispositivos suscriptos al tema "pagos" lo recibirán
                    Topic = "pagos"
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"✅ Notificación enviada correctamente: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al enviar notificación Firebase: {ex.Message}");
            }
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
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                int idUsuario = int.Parse(claim.Value);
                repo.Baja(id, idUsuario);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
