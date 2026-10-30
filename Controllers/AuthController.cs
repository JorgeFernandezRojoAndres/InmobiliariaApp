using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using InmobiliariaApp.Models;   // Para acceder a Usuario, RolUsuario y tus ViewModels
using InmobiliariaApp.Models.ViewModels;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Controllers
{
    public class AuthController : Controller   // 👈 IMPORTANTE: hereda de Controller
    {
        private readonly IRepoUsuario _repoUsuario;

        public AuthController(IRepoUsuario repoUsuario)
        {
            _repoUsuario = repoUsuario;
        }

        // ✅ GET: /Auth/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // ✅ POST: /Auth/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UsuarioRegistroViewModel model, IFormFile? avatar)
        {
            if (ModelState.IsValid)
            {
                // ✅ El email debe ser único: no se permite registrar uno existente
                if (_repoUsuario.ObtenerPorEmail(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "El email ya está registrado.");
                    return View(model);
                }

                // Hash seguro de contraseña
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Manejo del avatar
                string avatarPath = "/avatars/default.png";
                if (avatar != null && avatar.Length > 0)
                {
                    // Crear la carpeta wwwroot/avatars si no existe
                    var avatarDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                    if (!Directory.Exists(avatarDir))
                    {
                        Directory.CreateDirectory(avatarDir);
                    }

                    var fileName = Guid.NewGuid() + Path.GetExtension(avatar.FileName);
                    var path = Path.Combine(avatarDir, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await avatar.CopyToAsync(stream);
                    }
                    avatarPath = "/avatars/" + fileName;
                }

                var usuario = new Usuario
                {
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    // 👑 Quien se registra solo queda como dueño/administrador de su inmobiliaria
                    Rol = RolUsuario.Administrador,
                    AvatarUrl = avatarPath,
                    // 🏠 Inmobiliaria propia: RepoUsuario.Crear la deja en su propio Id si es null,
                    //    así el nuevo usuario arranca con datos ajenos visibles.
                    InmobiliariaId = null
                };

                _repoUsuario.Crear(usuario);

                return RedirectToAction("Login");
            }

            return View(model);
        }


        // ✅ GET: /Auth/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

       // ✅ POST: /Auth/Login
[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (ModelState.IsValid)
    {
        var usuario = _repoUsuario.ObtenerPorEmail(model.Email);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
        {
            ModelState.AddModelError("", "Credenciales inválidas");
            return View(model);
        }

        // Crear claims (👈 agregado AvatarUrl también)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), // ✅ agregado
            new Claim(ClaimTypes.Name, usuario.Email),                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("FullName", usuario.Nombre + " " + usuario.Apellido),
                new Claim("AvatarUrl", usuario.AvatarUrl ?? "/avatars/default.png"),
                // 🏢 Inmobiliaria del usuario: la usan los repos para aislar los datos
                new Claim(InmobiliariaApp.Helpers.InmobiliariaContext.ClaimType,
                           (usuario.InmobiliariaId ?? usuario.Id).ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        return RedirectToAction("Index", "Home");
    }

    return View(model);
}

        // ✅ GET: /Auth/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
