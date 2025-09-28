using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using InmobiliariaApp.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class ContratosController : Controller
    {
        private readonly IRepoContrato repo;
        private readonly RepoPersona repoPersona;
        private readonly RepoInmueble repoInmueble;
        private readonly IRepoPago repoPago;

        public ContratosController(IRepoContrato repo, RepoPersona repoPersona, RepoInmueble repoInmueble, IRepoPago repoPago)
        {
            this.repo = repo;
            this.repoPersona = repoPersona;
            this.repoInmueble = repoInmueble;
            this.repoPago = repoPago;
        }

        public IActionResult Index(DateTime? inicio, DateTime? fin)
        {
            IList<Contrato> contratos;

            if (inicio.HasValue && fin.HasValue && inicio <= fin)
            {
                contratos = repo.ObtenerVigentesEntre(inicio.Value, fin.Value);
                ViewData["Title"] = $"Contratos vigentes entre {inicio:dd/MM/yyyy} y {fin:dd/MM/yyyy}";
            }
            else
            {
                contratos = repo.ObtenerTodos();
                ViewData["Title"] = "Contratos";
            }

            return View(contratos);
        }
        public IActionResult Renovar(int id)
        {
            var original = repo.ObtenerPorId(id);
            if (original == null) return NotFound();

            // Nuevo contrato precargado
            var nuevo = new Contrato
            {
                IdInquilino = original.IdInquilino,
                IdInmueble = original.IdInmueble,
                MontoMensual = original.MontoMensual,
                Estado = "Vigente",
                // 🚨 Importante: no copiamos las fechas, dejamos sugerencia
                FechaInicio = original.FechaFin.AddDays(1),
                FechaFin = original.FechaFin.AddYears(1)
            };

            // Pasamos los selects de inquilinos e inmuebles
            CargarSelects(nuevo.IdInquilino, nuevo.IdInmueble);

            return View("Create", nuevo);
        }

        public IActionResult PorInmueble(int id)
        {
            var lista = repo.ObtenerPorInmueble(id);
            if (!lista.Any())
            {
                TempData["Mensaje"] = "Este inmueble no tiene contratos registrados.";
            }
            ViewData["Title"] = "Contratos del Inmueble";
            return View("Index", lista); // reutilizamos Index.cshtml
        }

        public IActionResult Details(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();

            var viewModel = new ContratoDetallesViewModel
            {
                Contrato = contrato,
                Pagos = repoPago.ObtenerPorContrato(id)
            };

            return View(viewModel);
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
                    // 👤 Obtener el ID del usuario logueado desde los Claims
                    var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null)
                    {
                        int idUsuario = int.Parse(claim.Value);
                        contrato.CreadoPor = idUsuario; // ✅ Guardamos quién lo creó
                    }

                    repo.Crear(contrato);
                    return RedirectToAction(nameof(Index));
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // 🔹 Si falla la validación o hay error, recargamos selects
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

            // 🔹 Obtener id del usuario logueado con validación
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId))
            {
                TempData["Error"] = "No se pudo identificar al usuario logueado.";
                return RedirectToAction(nameof(Index));
            }

            var idUsuario = int.Parse(claimId);

            // 🔹 Si ya venció -> marcar como vencido
            if (contrato.FechaFin <= DateTime.Now)
            {
                contrato.Estado = "Vencido";
                repo.Editar(contrato); // Solo actualizamos estado
            }
            else
            {
                // 🔹 Si aún está vigente -> marcar como Finalizado con auditoría
                repo.Eliminar(id, idUsuario);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult VigentesEntreFechas(DateTime? inicio, DateTime? fin)
        {
            if (!inicio.HasValue || !fin.HasValue)
            {
                ViewData["Title"] = "Buscar contratos vigentes entre fechas";
                return View(new List<Contrato>());
            }

            if (inicio > fin)
            {
                ModelState.AddModelError("", "La fecha de inicio no puede ser mayor que la fecha fin.");
                return View(new List<Contrato>());
            }

            var lista = repo.ObtenerVigentesEntre(inicio.Value, fin.Value);
            ViewData["Title"] = $"Contratos vigentes entre {inicio:dd/MM/yyyy} y {fin:dd/MM/yyyy}";
            return View(lista);
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

            var inmueblesLista = repoInmueble.Obtener()
                .Select(im => new
                {
                    Id = im.Id,
                    Display = $"{im.Direccion} - {im.TipoNombre} (${im.Precio}) - Propietario: {im.NombrePropietario}"
                }).ToList();

            ViewBag.Inquilinos = new SelectList(inquilinos, "Id", "Display", inquilinoId);
            ViewBag.Inmuebles = new SelectList(inmueblesLista, "Id", "Display", inmuebleId);
        }


    }
}
