using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using InmobiliariaApp.Models;
using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;
using System;
using System.Collections.Generic;

namespace InmobiliariaApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContratosApiController : ControllerBase
    {
        // ✅ Cambio: ahora usa la interfaz en lugar de la clase concreta
        private readonly IRepoContrato _repo;

        // ✅ Inyección correcta según Program.cs
        public ContratosApiController(IRepoContrato repo)
        {
            _repo = repo;
        }

        // 🔹 GET: api/Contratos/vigentes
        [HttpGet("vigentes")]
        public IActionResult GetVigentes()
        {
            try
            {
                var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
                if (claim == null)
                {
                    return Unauthorized(new
                    {
                        mensaje = "Token sin IdPropietario",
                        detalle = "El JWT no contiene el claim IdPropietario requerido."
                    });
                }

                int idPropietario = int.Parse(claim.Value);
                var contratos = _repo.ObtenerVigentesPorPropietario(idPropietario);

                if (contratos == null || !contratos.Any())
                    return Ok(new List<Contrato>()); // ✅ devuelve lista vacía en lugar de error

                return Ok(contratos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener contratos",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 GET: api/Contratos/{id}/pagos
        [HttpGet("{id}/pagos")]
        public IActionResult GetPagosPorContrato(int id)
        {
            try
            {
                var pagos = _repo.ObtenerPagosPorContrato(id);
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener pagos",
                    detalle = ex.Message
                });
            }
        }
    }
}
