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
using System.IO;
using System.Threading.Tasks;
using BCrypt.Net;
using MySqlConnector;






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
                // Validación de credenciales recibidas
                if (login == null ||
                    string.IsNullOrWhiteSpace(login.Email) ||
                    string.IsNullOrWhiteSpace(login.Clave))
                    return BadRequest(new { mensaje = "Faltan credenciales." });

                // Obtener propietario desde el repositorio
                var propietario = _repo.ObtenerPorEmail(login.Email);
                if (propietario == null)
                    return Unauthorized(new { mensaje = "Usuario no encontrado." });

                // 🔐 Verificación segura de clave (usando BCrypt si está activo en el sistema)
                bool claveValida;
                try
                {
                    // Verificar si la contraseña está cifrada con BCrypt
                    claveValida = BCrypt.Net.BCrypt.Verify(login.Clave, propietario.Clave);
                }
                catch (Exception ex)
                {
                    // Si no está cifrada con BCrypt, se compara en texto plano
                    // Log de la excepción (por si se quiere investigar más a fondo)
                    Console.WriteLine($"Error al verificar la contraseña (BCrypt): {ex.Message}");
                    claveValida = propietario.Clave == login.Clave; // Comparación en texto plano
                }

                // Si la clave no es válida, devolver un error de autenticación
                if (!claveValida)
                    return Unauthorized(new { mensaje = "Contraseña incorrecta." });

                // Obtener roles del propietario
                var roles = _repo.ObtenerRoles(propietario.Id);
                if (!roles.Contains("Propietario", StringComparer.OrdinalIgnoreCase))
                    return Forbid(); // Si no tiene el rol "Propietario", prohibir acceso

                // Crear los claims para el token JWT
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, propietario.Email),
            new Claim("IdPropietario", propietario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
        };

                // Agregar roles al token
                foreach (var rol in roles)
                    claims.Add(new Claim(ClaimTypes.Role, rol));

                // Generar el token JWT
                var token = _jwtHelper.GenerarToken(claims);

                // Generar la URL completa del avatar
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var avatarCompleto = string.IsNullOrEmpty(propietario.AvatarUrl)
                    ? ""
                    : $"{baseUrl}{propietario.AvatarUrl}";

                // Devolver la respuesta con el token y los datos del propietario
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
                // Manejo de errores en caso de fallos inesperados
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
        // 🔹 OLVIDÉ MI CONTRASEÑA
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { mensaje = "El correo electrónico es obligatorio." });

                var propietario = _repo.ObtenerPorEmail(email);
                if (propietario == null)
                    return NotFound(new { mensaje = "No se encontró una cuenta con ese correo." });

                // 🔑 Generar token único y asignarlo al propietario
                var token = Guid.NewGuid().ToString();
                propietario.TokenRecuperacion = token;

                // ✅ Guardar token directamente en la base (sin depender de Modificar)
                _repo.EjecutarSQL(
                    "UPDATE Personas SET TokenRecuperacion = @token WHERE ID = @id",
                    new Dictionary<string, object>
                    {
                { "@token", token },
                { "@id", propietario.Id }
                    });

                // 🔗 Construir enlace de recuperación
                var link = $"http://192.168.1.37:5027/api/Propietarios/reset-password?token={token}";

                // 📧 Enviar correo de recuperación
                var emailService = new EmailService(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build());

                await emailService.EnviarCorreoAsync(
                    email,
                    "Restablecer contraseña - Inmobiliaria 2025",
                    $@"
                <p>Hola {propietario.Nombre},</p>
                <p>Hacé clic en el siguiente enlace para restablecer tu contraseña:</p>
                <p><a href='{link}'>{link}</a></p>
                <p>Si no solicitaste el cambio, ignorá este mensaje.</p>
            "
                );

                return Ok(new { mensaje = "📬 Se envió un correo con el enlace para restablecer tu contraseña." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al enviar el correo de recuperación.",
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
        // 🔹 MOSTRAR PÁGINA DE RESTABLECIMIENTO AL ABRIR EL LINK DESDE EL CORREO
        [AllowAnonymous]
        [HttpGet("reset-password")]
        public IActionResult ResetPassword([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                const string errorHtml = @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8' />
            <title>Enlace inválido</title>
            <style>
                body { font-family:sans-serif; text-align:center; padding-top:40px; color:#c00; }
            </style>
        </head>
        <body>
            <h2>❌ Enlace inválido o expirado</h2>
            <p>El enlace de recuperación no es válido o ya fue usado.</p>
        </body>
        </html>";
                return Content(errorHtml, "text/html");
            }

            // ✅ Nueva versión con formulario HTML para cambiar la contraseña directamente
            var html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <title>Restablecer contraseña</title>
        <style>
            body {{
                font-family: 'Segoe UI', sans-serif;
                background-color: #f5f6f8;
                color: #333;
                text-align: center;
                padding-top: 60px;
            }}
            .card {{
                background: #fff;
                margin: auto;
                padding: 25px;
                border-radius: 12px;
                width: 320px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            }}
            input {{
                width: 100%;
                padding: 10px;
                margin-top: 10px;
                border: 1px solid #ccc;
                border-radius: 8px;
                font-size: 15px;
            }}
            button {{
                width: 100%;
                margin-top: 18px;
                padding: 10px;
                border: none;
                border-radius: 8px;
                background-color: #2E8B57;
                color: white;
                font-size: 16px;
                cursor: pointer;
            }}
            button:hover {{
                background-color: #256f45;
            }}
            .msg {{
                margin-top: 15px;
                font-weight: bold;
            }}
        </style>
        <script>
            async function enviarFormulario(event) {{
                event.preventDefault();
                const nuevaClave = document.getElementById('clave').value;
                const confirmar = document.getElementById('confirmar').value;
                const msg = document.getElementById('msg');

                if (!nuevaClave || !confirmar) {{
                    msg.innerText = '⚠️ Completá todos los campos';
                    msg.style.color = 'orange';
                    return;
                }}
                if (nuevaClave !== confirmar) {{
                    msg.innerText = '❌ Las contraseñas no coinciden';
                    msg.style.color = 'red';
                    return;
                }}

                msg.innerText = '⏳ Procesando...';
                msg.style.color = '#333';

                const data = {{
                    token: '{token}',
                    email: '',
                    nuevaClave: nuevaClave
                }};

                try {{
                    const resp = await fetch('/api/Propietarios/reset-password', {{
                        method: 'POST',
                        headers: {{ 'Content-Type': 'application/json' }},
                        body: JSON.stringify(data)
                    }});

                    const result = await resp.json();
                    if (resp.ok) {{
                        msg.innerText = '✅ ' + result.mensaje;
                        msg.style.color = 'green';
                    }} else {{
                        msg.innerText = '❌ ' + (result.mensaje || 'Error al cambiar la contraseña');
                        msg.style.color = 'red';
                    }}
                }} catch (err) {{
                    msg.innerText = '⚠️ Error de conexión: ' + err.message;
                    msg.style.color = 'red';
                }}
            }}
        </script>
    </head>
    <body>
        <div class='card'>
            <h2>Restablecer contraseña 🔑</h2>
            <form onsubmit='enviarFormulario(event)'>
                <input id='clave' type='password' placeholder='Nueva contraseña'>
                <input id='confirmar' type='password' placeholder='Confirmar contraseña'>
                <button type='submit'>Guardar</button>
                <div id='msg' class='msg'></div>
            </form>
        </div>
    </body>
    </html>";

            return Content(html, "text/html");
        }
       /// 🔹 RESETEAR CONTRASEÑA (desde el formulario web o app)
[AllowAnonymous]
[HttpPost("reset-password")]
public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
{
    try
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NuevaClave))
            return BadRequest(new { mensaje = "Datos incompletos para restablecer la contraseña." });

        // ✅ Buscar propietario con el token de recuperación
        Persona? propietario = null;
        using (var conn = new MySqlConnection(_repo.GetConnectionString()))
        {
            const string sql = "SELECT ID, Email, Clave FROM Personas WHERE TokenRecuperacion = @token LIMIT 1";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@token", dto.Token);
            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                propietario = new Persona
                {
                    Id = reader.GetInt32("ID"),
                    Email = reader.GetString("Email"),
                    Clave = reader["Clave"]?.ToString()
                };
            }
        }

        // ❌ Token inválido o ya usado
        if (propietario == null)
            return NotFound(new { mensaje = "❌ Enlace inválido o expirado." });

        // 🔒 Cifrar la nueva contraseña
        string nuevaClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaClave);

        // ✅ Actualizar contraseña y limpiar el token
        _repo.EjecutarSQL(
            "UPDATE Personas SET Clave = @clave, TokenRecuperacion = NULL WHERE ID = @id",
            new Dictionary<string, object>
            {
                { "@clave", nuevaClaveHash },
                { "@id", propietario.Id }
            });

        return Ok(new { mensaje = "✅ Contraseña restablecida correctamente." });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            mensaje = "Error al restablecer la contraseña.",
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
        
        // 🔹 DTO Cambio de Clave
        public class ResetPasswordDto
        {
            public string Email { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public string NuevaClave { get; set; } = string.Empty;
        }

        // 🔹 DTO Login interno
        public class LoginView
        {
            public string Email { get; set; } = string.Empty;
            public string Clave { get; set; } = string.Empty;
        }
    }
}
