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
        private readonly IRepoContrato _repo;

        public ContratosApiController(IRepoContrato repo)
        {
            _repo = repo;
        }

        // 🔹 GET: api/contratos/vigentes
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
                    return Ok(new List<Contrato>());

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

        // 🔹 GET: api/contratos/{id}/pagos
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

        // ⚖️ POST: api/contratos/rescindir/{id}
        [HttpPost("rescindir/{id}")]
        public IActionResult RescindirContrato(int id)
        {
            try
            {
                var contrato = _repo.ObtenerPorId(id);
                if (contrato == null)
                    return NotFound(new { mensaje = "Contrato no encontrado." });

                if (contrato.Estado != "Vigente")
                    return BadRequest(new { mensaje = "Solo los contratos vigentes pueden rescindirse." });

                var hoy = DateTime.Now;
                if (hoy >= contrato.FechaFin)
                    return BadRequest(new { mensaje = "El contrato ya finalizó." });

                // 🧮 Calcular meses restantes y multa
                int mesesRestantes = ((contrato.FechaFin.Year - hoy.Year) * 12) + contrato.FechaFin.Month - hoy.Month;
                if (mesesRestantes < 1) mesesRestantes = 1;

                decimal multa = contrato.MontoMensual * 0.5m * mesesRestantes;

                // 👤 Auditoría
                var claim = User.Claims.FirstOrDefault(c => c.Type == "IdPropietario");
                if (claim != null)
                {
                    contrato.TerminadoPor = int.Parse(claim.Value);
                }

                contrato.Estado = "Rescindido";
                contrato.FechaRescision = hoy;
                contrato.MontoMulta = multa;

                _repo.Editar(contrato);

                return Ok(new
                {
                    mensaje = $"Contrato rescindido correctamente.",
                    multa = multa.ToString("N2"),
                    contratoId = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al rescindir contrato",
                    detalle = ex.Message
                });
            }
        }
    }
}
