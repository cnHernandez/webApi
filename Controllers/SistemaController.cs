using Microsoft.AspNetCore.Mvc;
using ApiSwagger.Data;
using ApiSwagger.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiSwagger.Controllers
{
    [ApiController]
    [Route("api/sistema")]
    public class SistemaController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public SistemaController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/sistema/ultimo-procesamiento
        [HttpGet("ultimo-procesamiento")]
        public async Task<IActionResult> ObtenerUltimoProcesamiento()
        {
            try
            {
                var ultimoProcesamiento = await _context.ProcesamientosKilometraje
                    .OrderByDescending(p => p.FechaProcesamiento)
                    .FirstOrDefaultAsync();

                if (ultimoProcesamiento == null)
                {
                    return NotFound(new { mensaje = "No se ha realizado ningún procesamiento de kilometrajes aún." });
                }

                var resultado = new UltimoProcesamientoDto
                {
                    FechaUltimoArchivo = ultimoProcesamiento.FechaUltimoArchivo,
                    NombreUltimoArchivo = ultimoProcesamiento.NombreUltimoArchivo,
                    FechaProcesamiento = ultimoProcesamiento.FechaProcesamiento,
                    ArchivosProceados = ultimoProcesamiento.ArchivosProceados,
                    ColectivosActualizados = ultimoProcesamiento.ColectivosActualizados
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener información del último procesamiento", error = ex.Message });
            }
        }

        // GET /api/sistema/estado
        [HttpGet("estado")]
        public async Task<IActionResult> ObtenerEstadoSistema()
        {
            try
            {
                var totalColectivos = await _context.Colectivos.CountAsync();
                var totalUsuarios = await _context.Usuarios.CountAsync();
                
                var ultimoProcesamiento = await _context.ProcesamientosKilometraje
                    .OrderByDescending(p => p.FechaProcesamiento)
                    .FirstOrDefaultAsync();

                var estadoSistema = new
                {
                    TotalColectivos = totalColectivos,
                    TotalUsuarios = totalUsuarios,
                    UltimoProcesamiento = ultimoProcesamiento != null ? new UltimoProcesamientoDto
                    {
                        FechaUltimoArchivo = ultimoProcesamiento.FechaUltimoArchivo,
                        NombreUltimoArchivo = ultimoProcesamiento.NombreUltimoArchivo,
                        FechaProcesamiento = ultimoProcesamiento.FechaProcesamiento,
                        ArchivosProceados = ultimoProcesamiento.ArchivosProceados,
                        ColectivosActualizados = ultimoProcesamiento.ColectivosActualizados
                    } : null
                };

                return Ok(estadoSistema);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estado del sistema", error = ex.Message });
            }
        }
    }
}