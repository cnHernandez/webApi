using Microsoft.AspNetCore.Mvc;
using ApiSwagger.Models;
using ApiSwagger.Data;
using ApiSwagger.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiSwagger.Controllers.CambioAceite
{
    [ApiController]
    [Route("api/cambioaceite")]
    public class CambioAceiteController : ControllerBase
    {
        private readonly AppDbContext _context;
    public CambioAceiteController(ApiSwagger.Data.AppDbContext context)
        {
            _context = context;
        }

        // POST: api/cambioaceite
        [HttpPost]
        public async Task<IActionResult> RegistrarCambioAceite([FromBody] CambioAceiteDto dto)
        {
            var colectivo = await _context.Colectivos.FindAsync(dto.ColectivoId);
            if (colectivo == null)
                return NotFound($"No existe el colectivo con id {dto.ColectivoId}");

            var cambio = new ApiSwagger.Models.CambioAceite
            {
                ColectivoId = dto.ColectivoId,
                Fecha = dto.Fecha,
                Kilometros = dto.Kilometros,
                FiltrosCambiados = dto.FiltrosCambiados
            };
            _context.CambiosAceite.Add(cambio);
            await _context.SaveChangesAsync();
            dto.Id = cambio.Id;
            return CreatedAtAction(null, dto);
        }

        // GET: api/cambioaceite/historial/{colectivoId}
        [HttpGet("historial/{colectivoId}")]
        public async Task<ActionResult<IEnumerable<CambioAceiteDto>>> HistorialPorColectivo(int colectivoId)
        {
            var historial = await _context.CambiosAceite
                .Where(c => c.ColectivoId == colectivoId)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new CambioAceiteDto
                {
                    Id = c.Id,
                    ColectivoId = c.ColectivoId,
                    Fecha = c.Fecha,
                    Kilometros = c.Kilometros,
                    FiltrosCambiados = c.FiltrosCambiados
                })
                .ToListAsync();
            return Ok(historial);
        }
    }
}
