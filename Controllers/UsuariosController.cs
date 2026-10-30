using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Helpers;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly IRepoUsuario _repoUsuario;   // ✅ Usar la interfaz
        private readonly IInmobiliariaContext _inmobiliaria;

        public UsuariosController(IRepoUsuario repoUsuario, IInmobiliariaContext inmobiliaria)  // ✅ Recibir la interfaz
        {
            _repoUsuario = repoUsuario;
            _inmobiliaria = inmobiliaria;
        }

        public IActionResult Index()
        {
            var usuarios = _repoUsuario.ObtenerTodos();
            return View(usuarios);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Crear(Usuario usuario, IFormFile? AvatarFile) 
{
    if (!ModelState.IsValid) return View(usuario);

    if (AvatarFile != null && AvatarFile.Length > 0)
    {
        // ✅ Asegurar que la carpeta exista
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // ✅ Generar nombre único para el archivo
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            AvatarFile.CopyTo(stream);
        }

        usuario.AvatarUrl = "/avatars/" + fileName;
    }
    else
    {
        // ✅ Siempre asignar default si no se sube avatar
        usuario.AvatarUrl = "/avatars/default.png";
    }

    // ✅ Hashear password antes de guardar
    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

    // 🏢 El empleado siempre entra a la inmobiliaria del administrador que lo crea
    //    (se pisa cualquier InmobiliariaId que venga del formulario).
    usuario.InmobiliariaId = _inmobiliaria.InmobiliariaId;

    _repoUsuario.Crear(usuario);

    TempData["Success"] = "Usuario creado con éxito.";
    return RedirectToAction(nameof(Index));
}


        public IActionResult Editar(int id)
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            // 🔒 Solo se gestionan usuarios de la propia inmobiliaria
            if (usuario.InmobiliariaId != _inmobiliaria.InmobiliariaId) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Usuario usuario, IFormFile? AvatarFile, string? NuevaPassword)
        {
            if (!ModelState.IsValid) return View(usuario);

            var usuarioExistente = _repoUsuario.ObtenerPorId(id);
            if (usuarioExistente == null) return NotFound();
            // 🔒 Solo se gestionan usuarios de la propia inmobiliaria
            if (usuarioExistente.InmobiliariaId != _inmobiliaria.InmobiliariaId) return NotFound();

            // ✅ Contraseña: mantener la actual si no se ingresa nueva
            if (!string.IsNullOrWhiteSpace(NuevaPassword))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NuevaPassword);
            else
                usuario.PasswordHash = usuarioExistente.PasswordHash;

            // ✅ Avatar: subir nuevo si corresponde
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    AvatarFile.CopyTo(stream);
                }

                usuario.AvatarUrl = "/avatars/" + uniqueFileName;
            }
            else
            {
                usuario.AvatarUrl = string.IsNullOrWhiteSpace(usuarioExistente.AvatarUrl)
                    ? "/avatars/default.png"
                    : usuarioExistente.AvatarUrl;
            }

            _repoUsuario.Actualizar(usuario);
            TempData["Success"] = "Usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Eliminar(int id)
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            // 🔒 Solo se gestionan usuarios de la propia inmobiliaria
            if (usuario.InmobiliariaId != _inmobiliaria.InmobiliariaId) return NotFound();
            return View(usuario);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null || usuario.InmobiliariaId != _inmobiliaria.InmobiliariaId) return NotFound();

            _repoUsuario.Eliminar(id);
            TempData["Success"] = "Usuario eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
