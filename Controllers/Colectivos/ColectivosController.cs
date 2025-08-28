using Microsoft.AspNetCore.Mvc;
using ApiSwagger.Data;
using ApiSwagger.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSwagger.Controllers.Colectivos
{
    [ApiController]
    [Route("api/colectivos")]
    public class ColectivosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ColectivosController(AppDbContext context)
        {
            _context = context;
        }

        // GET /colectivos
        [HttpGet]
        public async Task<IActionResult> GetColectivos()
        {
            var colectivos = await _context.Colectivos.ToListAsync();
            return Ok(colectivos);
        }

        // GET /colectivos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetColectivo(int id)
        {
            var colectivo = await _context.Colectivos.FindAsync(id);
            if (colectivo == null) return NotFound();
            return Ok(colectivo);
        }

        // POST /colectivos
        [HttpPost]
        public async Task<IActionResult> CrearColectivo([FromBody] Colectivo colectivo)
        {
            _context.Colectivos.Add(colectivo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetColectivo), new { id = colectivo.IdColectivo }, colectivo);
        }

        // PATCH /colectivos/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] Colectivo datos)
        {
            var colectivo = await _context.Colectivos.FindAsync(id);
            if (colectivo == null) return NotFound();
            colectivo.Estado = datos.Estado;
            await _context.SaveChangesAsync();
            return Ok(colectivo);
        }
    }
}
