using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly RepoInmueble _repoInmueble;

        public HomeController(RepoInmueble repoInmueble)
        {
            _repoInmueble = repoInmueble;
        }

        public IActionResult Index()
        {
            // 🏢 Inmuebles REALES de la inmobiliaria del usuario logueado.
            //    RepoInmueble ya filtra por inmobiliaria: una inmobiliaria nueva
            //    no ve los datos de prueba ni los de otras inmobiliarias.
            var propiedades = _repoInmueble.Obtener();

            // Leer la cookie
            string? ultimaPropiedadVistaId = Request.Cookies["ultimaPropiedadVista"];
            if (!string.IsNullOrEmpty(ultimaPropiedadVistaId) && int.TryParse(ultimaPropiedadVistaId, out int idPropiedad))
            {
                var propiedadFavorita = propiedades.FirstOrDefault(p => p.Id == idPropiedad);
                if (propiedadFavorita != null)
                {
                    ViewBag.MensajeCookie = $"Tu última preferencia es: {propiedadFavorita.Direccion}.";
                }
                else
                {
                    ViewBag.MensajeCookie = "No hemos encontrado la propiedad favorita registrada.";
                }
            }
            else
            {
                ViewBag.MensajeCookie = "No hemos registrado tu preferencia de propiedad aún.";
            }

            return View(propiedades);
        }

        [HttpPost]
        public IActionResult BorrarFavorita()
        {
            Response.Cookies.Append("ultimaPropiedadVista", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                IsEssential = true
            });

            TempData["MensajeBorrado"] = "Se ha eliminado tu preferencia de propiedad.";
            return RedirectToAction("Index");
        }

        public IActionResult MarcarFavorita(int id)
        {
            Response.Cookies.Append("ultimaPropiedadVista", id.ToString(), new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true
            });

            return RedirectToAction("Index");
        }

        // 🔹 Acción Privacy para evitar error 404 en el menú
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
