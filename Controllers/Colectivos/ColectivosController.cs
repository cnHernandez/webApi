
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

        // PUT /colectivos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarColectivo(int id, [FromBody] Colectivo datos)
        {
            var colectivo = await _context.Colectivos.FindAsync(id);
            if (colectivo == null) return NotFound();

            colectivo.NroColectivo = datos.NroColectivo;
            colectivo.Patente = datos.Patente;
            colectivo.Modelo = datos.Modelo;
            colectivo.Estado = datos.Estado;
            colectivo.VtoVTV = datos.VtoVTV;
            colectivo.Kilometraje = datos.Kilometraje;

            await _context.SaveChangesAsync();
            return Ok(colectivo);
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

        // GET /colectivos/{nroColectivo}/historial-vtv
        [HttpGet("{nroColectivo}/historial-vtv")]
        public async Task<IActionResult> GetHistorialVtv(int nroColectivo)
        {
            // Buscar el colectivo por NroColectivo (string)
            string nroColectivoStr = nroColectivo.ToString();
            var colectivo = await _context.Colectivos.FirstOrDefaultAsync(c => c.NroColectivo == nroColectivoStr);
            if (colectivo == null)
                return NotFound();

            var historial = await _context.HistorialesVtv
                .Where(h => h.IdColectivo == colectivo.IdColectivo)
                .OrderByDescending(h => h.FechaRealizacion)
                .Select(h => new Dtos.HistorialVtvDto
                {
                    Id = h.Id,
                    IdColectivo = h.IdColectivo,
                    FechaRealizacion = h.FechaRealizacion,
                    FechaVencimiento = h.FechaVencimiento
                })
                .ToListAsync();
            if (historial == null || historial.Count == 0)
                return NotFound();
            return Ok(historial);
        }

        // POST /colectivos/{nroColectivo}/historial-vtv
        [HttpPost("{nroColectivo}/historial-vtv")]
        public async Task<IActionResult> RegistrarVtv(int nroColectivo, [FromBody] Dtos.HistorialVtvDto dto)
        {
            // Buscar colectivo por NroColectivo (string)
            string nroColectivoStr = nroColectivo.ToString();
            var colectivo = await _context.Colectivos.FirstOrDefaultAsync(c => c.NroColectivo == nroColectivoStr);
            if (colectivo == null)
                return NotFound($"No existe el colectivo {nroColectivo}");

            // Calcular fecha de vencimiento (1 año después de la realización)
            var fechaRealizacion = dto.FechaRealizacion;
            var fechaVencimiento = fechaRealizacion.AddYears(1);

            // Guardar historial usando IdColectivo como FK
            var historial = new HistorialVtv
            {
                IdColectivo = colectivo.IdColectivo,
                FechaRealizacion = fechaRealizacion,
                FechaVencimiento = fechaVencimiento
            };
            _context.HistorialesVtv.Add(historial);

            // Actualizar vencimiento en el colectivo
            colectivo.VtoVTV = DateOnly.FromDateTime(fechaVencimiento);
            await _context.SaveChangesAsync();

            // Devolver ambos: historial y colectivo actualizado
            return Ok(new {
                historial = new Dtos.HistorialVtvDto
                {
                    Id = historial.Id,
                    IdColectivo = historial.IdColectivo,
                    FechaRealizacion = historial.FechaRealizacion,
                    FechaVencimiento = historial.FechaVencimiento
                },
                colectivo
            });
        }
    }
}
