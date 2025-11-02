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
                if (propietario == null)
                    return Unauthorized(new { mensaje = "Usuario no encontrado." });

                // 🔐 Verificación segura de clave (usa BCrypt si está activo en tu sistema)
                bool claveValida;
                try
                {
                    claveValida = BCrypt.Net.BCrypt.Verify(login.Clave, propietario.Clave);
                }
                catch
                {
                    // Si no está encriptada, compara texto plano
                    claveValida = propietario.Clave == login.Clave;
                }

                if (!claveValida)
                    return Unauthorized(new { mensaje = "Contraseña incorrecta." });

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

                propietario.Nombre = datos.Nombre ?? propietario.Nombre;
                propietario.Apellido = datos.Apellido ?? propietario.Apellido;
                propietario.Telefono = datos.Telefono ?? propietario.Telefono;

                bool emailCambiado = false;
                if (!string.IsNullOrEmpty(datos.Email) && datos.Email != propietario.Email)
                {
                    propietario.Email = datos.Email;
                    emailCambiado = true;
                }

                _repo.Modificar(propietario);

                string? nuevoToken = null;
                if (emailCambiado)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, propietario.Email),
                        new Claim("IdPropietario", propietario.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

                    var roles = _repo.ObtenerRoles(propietario.Id);
                    foreach (var rol in roles)
                        claims.Add(new Claim(ClaimTypes.Role, rol));

                    nuevoToken = _jwtHelper.GenerarToken(claims);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var avatarCompleto = string.IsNullOrEmpty(propietario.AvatarUrl)
                    ? ""
                    : $"{baseUrl}{propietario.AvatarUrl}";

                return Ok(new
                {
                    mensaje = emailCambiado
                        ? "Perfil actualizado y token regenerado por cambio de email."
                        : "Perfil actualizado correctamente.",
                    token = nuevoToken,
                    propietario = new
                    {
                        propietario.Id,
                        propietario.Documento,
                        propietario.Nombre,
                        propietario.Apellido,
                        propietario.Email,
                        propietario.Telefono,
                        AvatarUrl = avatarCompleto
                    }
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
        // 🔹 CAMBIAR CONTRASEÑA
[HttpPut("cambiar-clave")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public IActionResult CambiarClave([FromBody] CambioClaveDto dto)
{
    try
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { mensaje = "Token inválido o expirado." });

        var propietario = _repo.ObtenerPorEmail(email);
        if (propietario == null)
            return NotFound(new { mensaje = "Propietario no encontrado." });

        // 🔐 Verificar contraseña actual
        bool claveValida;
        try
        {
            claveValida = BCrypt.Net.BCrypt.Verify(dto.ClaveActual, propietario.Clave);
        }
        catch
        {
            claveValida = propietario.Clave == dto.ClaveActual;
        }

        if (!claveValida)
            return BadRequest(new { mensaje = "La contraseña actual es incorrecta." });

        // 🔄 Generar hash de la nueva clave
        string nuevaClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaClave);
        propietario.Clave = nuevaClaveHash;

        // 💾 Guardar cambios con control de errores
        try
        {
            _repo.Modificar(propietario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al guardar la nueva contraseña.", detalle = ex.Message });
        }

        return Ok(new { mensaje = "Contraseña actualizada correctamente." });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error interno al cambiar contraseña.", detalle = ex.Message });
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
