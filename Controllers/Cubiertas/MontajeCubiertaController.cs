using Microsoft.AspNetCore.Mvc;
using ApiSwagger.Data;
using ApiSwagger.Models;
using ApiSwagger.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiSwagger.Controllers.Cubiertas
{
    [ApiController]
    [Route("api/montajes")]
    public class MontajeCubiertaController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MontajeCubiertaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/montajes/historialcubierta/{idCubierta}
        [HttpGet("historialcubierta/{idCubierta}")]
        public async Task<IActionResult> GetHistorialMontajeCubierta(int idCubierta)
        {
            var montajes = await _context.MontajesCubierta
                .Include(m => m.Cubierta)
                .Include(m => m.Colectivo)
                .Include(m => m.UbicacionCubierta)
                .Where(m => m.IdCubierta == idCubierta)
                .OrderByDescending(m => m.FechaMontaje)
                .ToListAsync();
            if (montajes == null || montajes.Count == 0)
                return NotFound();
            var historial = montajes.Select(m => new ApiSwagger.Dtos.HistorialMontajeCubiertaDto
            {
                NroSerieCubierta = m.Cubierta?.NroSerie ?? string.Empty,
                NroColectivo = m.Colectivo?.NroColectivo ?? string.Empty,
                DescripcionUbicacion = m.UbicacionCubierta?.Descripcion ?? string.Empty,
                FechaMontaje = m.FechaMontaje,
                MotivoCambio = m.MotivoCambio ?? string.Empty,
                FechaDesinstalacion = m.FechaDesinstalacion
            }).ToList();
            return Ok(historial);
        }
        // POST: api/montajes
        [HttpPost]
        public async Task<IActionResult> CrearMontaje([FromBody] MontajeCubiertaDto dto)
        {
            // Desasignar cubierta anterior si existe
            var montajeActual = await _context.MontajesCubierta
                .Where(m => m.IdColectivo == dto.IdColectivo && m.IdUbicacion == dto.IdUbicacion && m.FechaDesinstalacion == null)
                .FirstOrDefaultAsync();
            if (montajeActual != null)
            {
                montajeActual.FechaDesinstalacion = DateTime.Now;
                // Guardar motivo solo en el montaje desinstalado
                montajeActual.MotivoCambio = dto.MotivoCambio;
                // Desasignar la cubierta anterior
                var cubiertaAnterior = await _context.Cubiertas.FindAsync(montajeActual.IdCubierta);
                if (cubiertaAnterior != null)
                {
                    cubiertaAnterior.IdColectivo = 0;
                    cubiertaAnterior.Ubicacion = null;
                    var entry = _context.Entry(cubiertaAnterior);
                    entry.Property("UbicacionIdUbicacion").CurrentValue = null;
                    _context.Cubiertas.Update(cubiertaAnterior);
                }
                _context.MontajesCubierta.Update(montajeActual);
            }
            // Crear nuevo montaje SIN motivo de cambio
            var cubierta = await _context.Cubiertas.FindAsync(dto.IdCubierta);
            var colectivo = await _context.Colectivos.FindAsync(dto.IdColectivo);
            var ubicacion = await _context.UbicacionesCubierta.FindAsync(dto.IdUbicacion);
            var nuevoMontaje = new MontajeCubierta
            {
                IdCubierta = dto.IdCubierta,
                IdColectivo = dto.IdColectivo,
                IdUbicacion = dto.IdUbicacion,
                MotivoCambio = "", // No se guarda motivo en el nuevo montaje
                FechaMontaje = DateTime.Now,
                Cubierta = cubierta!,
                Colectivo = colectivo!,
                UbicacionCubierta = ubicacion!
            };
            // Actualizar la cubierta con el colectivo y ubicación asignados
            if (cubierta != null)
            {
                cubierta.IdColectivo = dto.IdColectivo;
                cubierta.Ubicacion = ubicacion;
                _context.Cubiertas.Update(cubierta);
            }
            _context.MontajesCubierta.Add(nuevoMontaje);
            await _context.SaveChangesAsync();
            return Ok("Montaje guardado correctamente");
        }

        // GET: api/montajes/actual/{idColectivo}/{idUbicacion}
        [HttpGet("actual/{idColectivo}/{idUbicacion}")]
        public async Task<IActionResult> ConsultarMontajeActual(int idColectivo, int idUbicacion)
        {
            var montaje = await _context.MontajesCubierta
                .Include(m => m.Cubierta)
                .Where(m => m.IdColectivo == idColectivo && m.IdUbicacion == idUbicacion && m.FechaDesinstalacion == null)
                .FirstOrDefaultAsync();
            if (montaje == null) return Ok(null);
            return Ok(new MontajeCubiertaInfoDto
            {
                IdMontaje = montaje.IdMontaje,
                IdCubierta = montaje.IdCubierta,
                IdColectivo = montaje.IdColectivo ?? 0,
                IdUbicacion = montaje.IdUbicacion ?? 0,
                FechaMontaje = montaje.FechaMontaje,
                MotivoCambio = montaje.MotivoCambio,
                FechaDesinstalacion = montaje.FechaDesinstalacion,
                NroSerieCubierta = montaje.Cubierta?.NroSerie ?? string.Empty
            });
        }
    }
}
