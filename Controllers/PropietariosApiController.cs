using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using InmobiliariaApp.Helpers; // ✅ helper JWT
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace InmobiliariaApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropietariosApiController : ControllerBase
    {
        private readonly RepoPersona _repo;
        private readonly JwtHelper _jwtHelper;

        public PropietariosApiController(RepoPersona repo, JwtHelper jwtHelper)
        {
            _repo = repo;
            _jwtHelper = jwtHelper;
        }

        // 🔹 LOGIN para Propietarios desde la app Android
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginView login)
        {
            try
            {
                if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Clave))
                    return BadRequest(new { mensaje = "Faltan credenciales." });

                var propietario = _repo.ObtenerPorEmail(login.Email);

                if (propietario == null || propietario.Clave != login.Clave)
                    return Unauthorized(new { mensaje = "Credenciales inválidas." });

                var roles = _repo.ObtenerRoles(propietario.Id);

                if (!roles.Contains("Propietario", StringComparer.OrdinalIgnoreCase))
                    return Forbid();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, propietario.Email),
                    new Claim("IdPropietario", propietario.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var rol in roles)
                    claims.Add(new Claim(ClaimTypes.Role, rol));

                var token = _jwtHelper.GenerarToken(claims);

                return Ok(new
                {
                    token,
                    propietario = new
                    {
                        propietario.Id,
                        propietario.Documento,
                        propietario.Nombre,
                        propietario.Apellido,
                        propietario.Email,
                        propietario.Telefono,
                        propietario.AvatarUrl,
                        roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al iniciar sesión.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ SUBIR AVATAR (solo accesible con JWT válido)
        [HttpPost("subirAvatar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SubirAvatar([FromForm] IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { mensaje = "Archivo vacío o nulo" });

            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", nombreArchivo);

            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var email = User.Identity!.Name!;
            var propietario = _repo.ObtenerPorEmail(email);
            if (propietario == null)
                return NotFound(new { mensaje = "Propietario no encontrado." });

            propietario.AvatarUrl = $"/avatars/{nombreArchivo}";
            _repo.Modificar(propietario);

            return Ok(new { avatarUrl = propietario.AvatarUrl });
        }

        // 🔸 DTO dentro de la clase
        public class LoginView
        {
            public string Email { get; set; } = string.Empty;
            public string Clave { get; set; } = string.Empty;
        }
    }
}
