using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaApp.Repository;

namespace InmobiliariaApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class InquilinosApiController : ControllerBase
    {
        private readonly RepoPersona _repo;

        public InquilinosApiController(RepoPersona repo)
        {
            _repo = repo;
        }

        [HttpGet("con-inmueble")]
        [Authorize]
        public IActionResult GetConInmueble()
        {
            try
            {
                var claim = User.FindFirst("IdPropietario");

                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return Unauthorized(new { mensaje = "Token inválido o falta el ID del usuario" });
                }

                int propietarioId = int.Parse(claim.Value);

                var lista = _repo.ObtenerInquilinosConInmueble(propietarioId);

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error obteniendo inquilinos con inmueble.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ NUEVO: detalle por ID para la app móvil
        [HttpGet("{idInquilino}")]
        [Authorize]
        public IActionResult GetById(int idInquilino)
        {
            try
            {
                var data = _repo.ObtenerInquilinoPorId(idInquilino);

                if (data == null)
                {
                    return NotFound(new { mensaje = "Inquilino no encontrado" });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error obteniendo detalle del inquilino",
                    detalle = ex.Message
                });
            }
        }
    }
}
