using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using InmobiliariaApp.Helpers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace InmobiliariaApp.Controllers.Api
{
    [Route("api/Propietarios")]
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

        // 🔹 LOGIN
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginView login)
        {
            try
            {
                if (login == null ||
                    string.IsNullOrWhiteSpace(login.Email) ||
                    string.IsNullOrWhiteSpace(login.Clave))
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

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var avatarCompleto = string.IsNullOrEmpty(propietario.AvatarUrl)
                    ? ""
                    : $"{baseUrl}{propietario.AvatarUrl}";

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
                        AvatarUrl = avatarCompleto,
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

        // 🔹 SUBIR AVATAR
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

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return Ok(new { avatarUrl = $"{baseUrl}{propietario.AvatarUrl}" });
        }

        // 🔹 OBTENER PERFIL
        [HttpGet("perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Perfil()
        {
            try
            {
                var email = User.Identity?.Name;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { mensaje = "Token inválido o expirado." });

                var propietario = _repo.ObtenerPorEmail(email);
                if (propietario == null)
                    return NotFound(new { mensaje = "Propietario no encontrado." });

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var avatarCompleto = string.IsNullOrEmpty(propietario.AvatarUrl)
                    ? ""
                    : $"{baseUrl}{propietario.AvatarUrl}";

                return Ok(new
                {
                    propietario.Id,
                    propietario.Documento,
                    propietario.Nombre,
                    propietario.Apellido,
                    propietario.Email,
                    propietario.Telefono,
                    AvatarUrl = avatarCompleto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al obtener perfil.",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 ACTUALIZAR PERFIL (PUT)
        [HttpPut("perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ActualizarPerfil([FromBody] Propietario datos)
        {
            try
            {
                var email = User.Identity?.Name;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { mensaje = "Token inválido o expirado." });

                var propietario = _repo.ObtenerPorEmail(email);
                if (propietario == null)
                    return NotFound(new { mensaje = "Propietario no encontrado." });

                // 🔸 Solo se actualizan los campos editables desde la app
                propietario.Nombre = datos.Nombre ?? propietario.Nombre;
                propietario.Apellido = datos.Apellido ?? propietario.Apellido;
                propietario.Telefono = datos.Telefono ?? propietario.Telefono;

                _repo.Modificar(propietario);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var avatarCompleto = string.IsNullOrEmpty(propietario.AvatarUrl)
                    ? ""
                    : $"{baseUrl}{propietario.AvatarUrl}";

                return Ok(new
                {
                    propietario.Id,
                    propietario.Documento,
                    propietario.Nombre,
                    propietario.Apellido,
                    propietario.Email,
                    propietario.Telefono,
                    AvatarUrl = avatarCompleto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al actualizar perfil.",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 DTO Login interno
        public class LoginView
        {
            public string Email { get; set; } = string.Empty;
            public string Clave { get; set; } = string.Empty;
        }
    }
}
