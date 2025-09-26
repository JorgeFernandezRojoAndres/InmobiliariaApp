using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace InmobiliariaApp.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly RepoUsuario _repoUsuario;

        public PerfilController(RepoUsuario repoUsuario)
        {
            _repoUsuario = repoUsuario;
        }

        // GET: /Perfil/Editar
        public IActionResult Editar()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Auth");

            var usuario = _repoUsuario.ObtenerPorEmail(email);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: /Perfil/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Usuario model, string? NuevaPassword, IFormFile? AvatarFile, bool eliminarAvatar = false)
        {
            var usuario = _repoUsuario.ObtenerPorId(model.Id);
            if (usuario == null) return NotFound();

            // Actualizar datos básicos
            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Email = model.Email;

            // Contraseña: mantener si no se cambia
            if (!string.IsNullOrWhiteSpace(NuevaPassword))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NuevaPassword);

            // Manejo de avatar
            if (eliminarAvatar)
            {
                usuario.AvatarUrl = null; // quedará default al leer
            }
            else if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                usuario.AvatarUrl = "/avatars/" + uniqueFileName;
            }

            // Guardar cambios en la BD
            _repoUsuario.Actualizar(usuario);

            // 🔹 Regenerar claims para que se actualicen en la sesión actual
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("FullName", usuario.Nombre + " " + usuario.Apellido),
                new Claim("AvatarUrl", usuario.AvatarUrl ?? "/avatars/default.png")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Editar));
        }
    }
}
