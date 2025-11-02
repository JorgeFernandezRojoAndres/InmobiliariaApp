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

        [Authorize(Roles = "Propietario,Administrador,Empleado")]
        [Authorize(Roles = "Propietario,Administrador,Empleado")]
        public IActionResult Index(DateTime? inicio, DateTime? fin, int? propietarioId)
        {
            IList<Contrato> contratos;

            // 👤 Obtener el ID del usuario logueado
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId))
            {
                TempData["Error"] = "No se pudo identificar al usuario logueado.";
                return RedirectToAction("Login", "Usuarios");
            }

            int userId = int.Parse(claimId);
            bool esPropietario = User.IsInRole("Propietario");

            if (esPropietario)
            {
                // 🔹 Solo contratos del propietario logueado
                contratos = repo.ObtenerTodos()
                    .Where(c => c.Inmueble != null && c.Inmueble.PropietarioId == userId)
                    .ToList();
                ViewBag.Propietarios = null;
            }
            else
            {
                // 🔹 Mostrar combo de propietarios
                var propietarios = repoPersona.ObtenerPropietarios();
                ViewBag.Propietarios = propietarios;
                ViewBag.PropietarioSeleccionado = propietarioId;

                contratos = repo.ObtenerTodos();

                // 🔹 Filtrar por propietario (solo si se selecciona)
                if (propietarioId.HasValue && propietarioId.Value > 0)
                {
                    contratos = contratos
                        .Where(c => c.Inmueble != null && c.Inmueble.PropietarioId == propietarioId.Value)
                        .ToList();
                }
            }

            // 🔹 Filtrar por fechas (si están definidas)
            if (inicio.HasValue && fin.HasValue && inicio <= fin)
            {
                contratos = contratos
                    .Where(c => c.FechaInicio >= inicio.Value && c.FechaFin <= fin.Value)
                    .ToList();
                ViewData["Title"] = $"Contratos entre {inicio:dd/MM/yyyy} y {fin:dd/MM/yyyy}";
            }
            else
            {
                ViewData["Title"] = esPropietario ? "Contratos" : "Contratos";
            }

            return View(contratos);
        }


        public IActionResult Renovar(int id)
{
    var original = repo.ObtenerPorId(id);
    if (original == null) return NotFound();

    // ✅ Solo se puede renovar contratos finalizados
    if (original.Estado != "Finalizado" && original.FechaFin > DateTime.Now)
    {
        TempData["Error"] = "Solo se pueden renovar contratos ya finalizados o vencidos.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ✅ Marcar finalizado
    original.Estado = "Finalizado";
    repo.Editar(original);

    // ✅ Crear nuevo contrato - misma lógica que API
    var nuevo = new Contrato
    {
        IdInquilino = original.IdInquilino,
        IdInmueble = original.IdInmueble,
        MontoMensual = original.MontoMensual,
        Estado = "Vigente",
        FechaInicio = original.FechaFin.AddDays(1),
        FechaFin = original.FechaFin.AddYears(1),
        ContratoOriginalId = original.Id
    };

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Rescindir(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null)
            {
                TempData["Error"] = "Contrato no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (contrato.Estado != "Vigente")
            {
                TempData["Error"] = "Solo los contratos vigentes pueden rescindirse.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var hoy = DateTime.Now;

            if (hoy >= contrato.FechaFin)
            {
                TempData["Error"] = "El contrato ya finalizó.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 🧮 Calcular meses restantes y multa (50% de los meses restantes)
            int mesesRestantes = ((contrato.FechaFin.Year - hoy.Year) * 12) + contrato.FechaFin.Month - hoy.Month;
            if (mesesRestantes < 1) mesesRestantes = 1;

            decimal multa = contrato.MontoMensual * 0.5m * mesesRestantes;

            // 👤 Auditoría
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                contrato.TerminadoPor = int.Parse(claim.Value);
            }

            contrato.Estado = "Rescindido";
            contrato.FechaRescision = hoy;
            contrato.MontoMulta = multa;

            repo.Editar(contrato);

            TempData["Mensaje"] = $"Contrato rescindido correctamente. Multa: ${multa:N2}";
            return RedirectToAction(nameof(Details), new { id });
        }




    }
}
