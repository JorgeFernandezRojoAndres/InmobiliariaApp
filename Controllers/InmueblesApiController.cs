using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using System.Security.Claims;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace InmobiliariaApp.Controllers.Api
{
    [Route("api/Inmuebles")]
    [ApiController]
    [Authorize] // ✅ Requiere JWT válido
    public class InmueblesApiController : ControllerBase
    {
        private readonly RepoInmueble _repoInmueble;

        public InmueblesApiController(RepoInmueble repoInmueble)
        {
            _repoInmueble = repoInmueble;
        }

        // 🔹 GET: /api/Inmuebles/alquilados
        [HttpGet("alquilados")]
        public IActionResult GetInmueblesAlquilados()
        {
            try
            {
                var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
                if (claim == null || !int.TryParse(claim.Value, out int idPropietario))
                    return Unauthorized(new { mensaje = "Token inválido o sin IdPropietario." });

                var inmuebles = _repoInmueble.ObtenerAlquiladosPorPropietario(idPropietario);
                return Ok(inmuebles ?? new List<Inmueble>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener los inmuebles alquilados.",
                    detalle = ex.Message
                });
            }
        }
        // 🔹 GET: /api/Inmuebles/misInmuebles
[HttpGet("misInmuebles")]
public IActionResult ObtenerMisInmuebles()
{
    try
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
        if (claim == null || !int.TryParse(claim.Value, out int idPropietario))
            return Unauthorized(new { mensaje = "Token inválido o sin IdPropietario." });

        var inmuebles = _repoInmueble.ObtenerPorPropietario(idPropietario);

        // ✅ Si hay inmuebles, agregar la URL completa de la imagen
        if (inmuebles != null)
        {
            foreach (var i in inmuebles)
            {
                if (!string.IsNullOrEmpty(i.ImagenUrl))
                {
                    // 🧩 Limpia rutas duplicadas
                    i.ImagenUrl = i.ImagenUrl
                        .Replace("/uploads/uploads/", "/uploads/")
                        .Replace("//uploads/", "/uploads/");

                    // 🌐 Si la ruta es relativa, la completamos con el dominio actual
                    if (!i.ImagenUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = i.ImagenUrl.TrimStart('/');
                        i.ImagenUrl = $"{Request.Scheme}://{Request.Host}/{path}";
                    }
                }
            }
        }

        return Ok(inmuebles ?? new List<Inmueble>());
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            mensaje = "Error al obtener los inmuebles del propietario.",
            detalle = ex.Message
        });
    }
}


        // 🔹 PUT JSON: /api/Inmuebles/{id}
        [HttpPut("{id}")]
        public IActionResult ActualizarInmueble(int id, [FromBody] Inmueble inmueble)
        {
            try
            {
                if (id != inmueble.Id)
                    return BadRequest(new { mensaje = "El ID del inmueble no coincide." });

                var filas = _repoInmueble.ModificacionConImagen(inmueble);


                if (filas > 0)
                    return Ok(new { mensaje = "✅ Inmueble actualizado correctamente." });
                else
                    return NotFound(new { mensaje = "No se encontró el inmueble para actualizar." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al actualizar el inmueble.",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 PUT MULTIPART: /api/Inmuebles/{id}/form 
        // ✅ Soporta imagen + datos del inmueble

        [HttpPut("{id}/form")]
        public async Task<IActionResult> ActualizarInmuebleForm(
            int id,
            [FromForm] Inmueble inmueble,
            [FromForm] IFormFile? imagen)
        {
            try
            {
                // 🧩 Debug inicial: IDs y campos básicos
                Console.WriteLine($"[DEBUG Controller] PUT /api/Inmuebles/{id}/form -> idParam={id}, inmueble.Id={(inmueble?.Id.ToString() ?? "null")}, Direccion={(inmueble?.Direccion ?? "null")}, TipoId={(inmueble?.TipoId.ToString() ?? "null")}, PropietarioId={(inmueble?.PropietarioId.ToString() ?? "null")}");

                // 🧩 Validar que el body no sea nulo
                if (inmueble == null)
                    return BadRequest(new { mensaje = "El cuerpo del formulario no contiene datos de inmueble." });

                // 🧩 Si el form no trae el Id (común en multipart), usar el de la ruta
                if (inmueble.Id == 0)
                    inmueble.Id = id;
                else if (id != inmueble.Id)
                    return BadRequest(new { mensaje = "El ID del inmueble no coincide." });

                // 🔹 Obtener PropietarioId desde el token (si no vino en el form)
                var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
                if (claim == null || !int.TryParse(claim.Value, out int idPropietario))
                    return Unauthorized(new { mensaje = "Token inválido o sin IdPropietario." });

                if (inmueble.PropietarioId == 0)
                    inmueble.PropietarioId = idPropietario;

                // 🔹 Procesar imagen si viene adjunta
                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"propietarios_{idPropietario}");
                    if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

                    var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
                    var rutaArchivo = Path.Combine(uploadsPath, nombreArchivo);

                    using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                        await imagen.CopyToAsync(stream);

                    inmueble.ImagenUrl = $"/uploads/propietarios_{idPropietario}/{nombreArchivo}";
                }

                // ✅ Validaciones previas antes de guardar
                if (inmueble.TipoId <= 0)
                    return BadRequest(new { mensaje = "TipoId inválido o no recibido desde el formulario." });
                if (inmueble.PropietarioId <= 0)
                    return BadRequest(new { mensaje = "PropietarioId no válido o no asignado." });

                // 🔹 Actualizar inmueble (usa método corregido del repo)
                var filas = _repoInmueble.ModificacionConImagen(inmueble);

                // 🔹 Resultado
                if (filas > 0)
                {
                    Console.WriteLine($"[DEBUG Controller] ✅ Inmueble actualizado correctamente (Id={inmueble.Id})");
                    return Ok(new { mensaje = "✅ Inmueble actualizado correctamente con imagen." });
                }
                else
                {
                    Console.WriteLine($"[WARN Controller] ⚠️ No se encontró el inmueble con Id={inmueble.Id}");
                    return NotFound(new { mensaje = "No se encontró el inmueble para actualizar." });
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"[ERROR Controller SQL] {sqlEx.Message}");
                return StatusCode(500, new
                {
                    mensaje = "Error SQL al actualizar el inmueble.",
                    detalle = sqlEx.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR Controller] {ex}");
                return StatusCode(500, new
                {
                    mensaje = "Error al actualizar el inmueble (form-data).",
                    detalle = ex.Message
                });
            }
        }




        // 🔹 POST: /api/Inmuebles/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImagen([FromForm] int idInmueble, [FromForm] IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
                return BadRequest(new { mensaje = "Debe enviarse una imagen válida." });

            try
            {
                var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
                if (claim == null || !int.TryParse(claim.Value, out int idPropietario))
                    return Unauthorized(new { mensaje = "Token inválido o sin IdPropietario." });

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"propietarios_{idPropietario}");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
                var rutaArchivo = Path.Combine(uploadsPath, nombreArchivo);

                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                    await imagen.CopyToAsync(stream);

                var rutaRelativa = $"/uploads/propietarios_{idPropietario}/{nombreArchivo}";
                _repoInmueble.GuardarImagen(idInmueble, rutaRelativa);

                return Ok(new { mensaje = "✅ Imagen subida correctamente.", url = rutaRelativa });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al subir la imagen.", detalle = ex.Message });
            }
        }
    }
}
