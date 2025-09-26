using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static List<Propiedad> _propiedades = new List<Propiedad>
        {
            new Propiedad { Id = 1, Direccion = "Calle Falsa 123", Precio = 150000, Dormitorios = 3, Descripcion = "Amplia casa en zona céntrica." },
            new Propiedad { Id = 2, Direccion = "Avenida Siempreviva 742", Precio = 200000, Dormitorios = 4, Descripcion = "Chalet con jardín y pileta." },
            new Propiedad { Id = 3, Direccion = "Pasaje Secreto 1", Precio = 90000, Dormitorios = 2, Descripcion = "Departamento acogedor, ideal para parejas." }
        };

        public IActionResult Index()
        {
            // Leer la cookie
            string? ultimaPropiedadVistaId = Request.Cookies["ultimaPropiedadVista"];
            if (!string.IsNullOrEmpty(ultimaPropiedadVistaId) && int.TryParse(ultimaPropiedadVistaId, out int idPropiedad))
            {
                var propiedadFavorita = _propiedades.FirstOrDefault(p => p.Id == idPropiedad);
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

            return View(_propiedades);
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
