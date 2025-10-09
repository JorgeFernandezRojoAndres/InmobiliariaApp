using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using System.Security.Claims;
using System.Linq;
using System;
using System.Collections.Generic;

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

        // 🔹 Endpoint consumido por Android
        // GET: /api/Inmuebles/alquilados
        [HttpGet("alquilados")]
        public IActionResult GetInmueblesAlquilados()
        {
            try
            {
                // ✅ Leer el IdPropietario desde el token JWT
                var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
                if (claim == null || !int.TryParse(claim.Value, out int idPropietario))
                {
                    return Unauthorized(new { mensaje = "Token inválido o sin IdPropietario." });
                }

                // ✅ Obtener inmuebles alquilados del propietario
                var inmuebles = _repoInmueble.ObtenerAlquiladosPorPropietario(idPropietario);

                if (inmuebles == null || inmuebles.Count == 0)
                    return Ok(new List<Inmueble>()); // lista vacía pero 200 OK

                return Ok(inmuebles);
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

        // 🔹 NUEVO ENDPOINT: misInmuebles → lista completa del propietario autenticado
        // GET: /api/Inmuebles/misInmuebles
        [HttpGet("misInmuebles")]
        public IActionResult ObtenerMisInmuebles()
        {
            try
            {
                // ✅ Extraer IdPropietario del token JWT
                var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
                if (claim == null || !int.TryParse(claim.Value, out int idPropietario))
                {
                    return Unauthorized(new { mensaje = "Token inválido o sin IdPropietario." });
                }

                // ✅ Obtener inmuebles del propietario (usa tu método existente)
                var inmuebles = _repoInmueble.ObtenerPorPropietario(idPropietario);

                if (inmuebles == null || inmuebles.Count == 0)
                    return Ok(new List<Inmueble>());

                return Ok(inmuebles);
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
    }
}
