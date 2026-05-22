using Microsoft.AspNetCore.Mvc;
using ApiSwagger.Data;
using ApiSwagger.Dtos;
using ApiSwagger.Models;
using ApiSwagger.Services;
using Microsoft.EntityFrameworkCore;

namespace ApiSwagger.Controllers.Colectivos
{
    [ApiController]
    [Route("api/colectivos")]
    public class ColectivosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ColectivoService _colectivoService;

        public ColectivosController(AppDbContext context, ColectivoService colectivoService)
        {
            _context = context;
            _colectivoService = colectivoService;
        }

        // PUT /colectivos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarColectivo(int id, [FromBody] GuardarColectivoDto datos)
        {
            try
            {
                var colectivo = await _colectivoService.ActualizarAsync(id, datos);
                var montajesActivos = await ObtenerMontajesActivosAsync(new[] { colectivo.IdColectivo });
                var numerosAsignados = await ObtenerNumerosAsignadosActualesAsync();
                return Ok(MapColectivo(colectivo, montajesActivos, numerosAsignados));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }


        // GET /colectivos
        [HttpGet]
        public async Task<IActionResult> GetColectivos()
        {
            var colectivos = await _colectivoService.ListarAsync();
            var montajesActivos = await ObtenerMontajesActivosAsync(colectivos.Select(c => c.IdColectivo));
            var numerosAsignados = await ObtenerNumerosAsignadosActualesAsync();
            return Ok(colectivos.Select(c => MapColectivo(c, montajesActivos, numerosAsignados)));
        }

        // GET /colectivos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetColectivo(int id)
        {
            var colectivo = await _colectivoService.ObtenerPorIdAsync(id);
            if (colectivo == null) return NotFound();
            var montajesActivos = await ObtenerMontajesActivosAsync(new[] { colectivo.IdColectivo });
            var numerosAsignados = await ObtenerNumerosAsignadosActualesAsync();
            return Ok(MapColectivo(colectivo, montajesActivos, numerosAsignados));
        }


        // POST /colectivos
        [HttpPost]
        public async Task<IActionResult> CrearColectivo([FromBody] GuardarColectivoDto colectivo)
        {
            try
            {
                var creado = await _colectivoService.CrearAsync(colectivo);
                var montajesActivos = await ObtenerMontajesActivosAsync(new[] { creado.IdColectivo });
                var numerosAsignados = await ObtenerNumerosAsignadosActualesAsync();
                return CreatedAtAction(nameof(GetColectivo), new { id = creado.IdColectivo }, MapColectivo(creado, montajesActivos, numerosAsignados));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // PATCH /colectivos/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] GuardarColectivoDto datos)
        {
            try
            {
                var colectivo = await _colectivoService.ActualizarAsync(id, datos);
                var montajesActivos = await ObtenerMontajesActivosAsync(new[] { colectivo.IdColectivo });
                var numerosAsignados = await ObtenerNumerosAsignadosActualesAsync();
                return Ok(MapColectivo(colectivo, montajesActivos, numerosAsignados));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
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

            // Calcular fechas
            var fechaRealizacion = dto.FechaRealizacion;
            int mesesVto = 12; // valor por defecto
            int anioActual = DateTime.Now.Year;
            int anioModelo = 0;
            if (!string.IsNullOrEmpty(colectivo.Modelo) && int.TryParse(colectivo.Modelo, out anioModelo))
            {
                if (anioActual - anioModelo >= 10)
                {
                    mesesVto = 4;
                }
                else
                {
                    mesesVto = 6;
                }
            }
            // Si no se puede parsear el modelo, dejar 12 meses como fallback
            var fechaVencimientoNueva = fechaRealizacion.AddMonths(mesesVto);

            // Guardar historial usando IdColectivo como FK y la fecha de vencimiento anterior
            var historial = new HistorialVtv
            {
                IdColectivo = colectivo.IdColectivo,
                FechaRealizacion = fechaRealizacion,
                FechaVencimiento = colectivo.VtoVTV != null
                    ? colectivo.VtoVTV.Value.ToDateTime(TimeOnly.MinValue)
                    : fechaRealizacion // o DateTime.MinValue si prefieres un valor por defecto
            };
            _context.HistorialesVtv.Add(historial);

            // Actualizar vencimiento en el colectivo
            colectivo.VtoVTV = DateOnly.FromDateTime(fechaVencimientoNueva);
            await _context.SaveChangesAsync();

            // Devolver ambos: historial y colectivo actualizado
            return Ok(new
            {
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
           // DELETE /colectivos/{id} (baja física)
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarColectivo(int id)
        {
            try
            {
                await _colectivoService.DarDeBajaAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET /colectivos/por-nro/{nroColectivo}
        [HttpGet("por-nro/{nroColectivo}")]
        public async Task<IActionResult> GetColectivoPorNro(string nroColectivo)
        {
            var colectivo = await _colectivoService.ObtenerPorNumeroAsync(nroColectivo);
            if (colectivo == null)
                return NotFound();
            var montajesActivos = await ObtenerMontajesActivosAsync(new[] { colectivo.IdColectivo });
            var numerosAsignados = await ObtenerNumerosAsignadosActualesAsync();
            return Ok(MapColectivo(colectivo, montajesActivos, numerosAsignados));
        }

        private async Task<HashSet<string>> ObtenerNumerosAsignadosActualesAsync()
        {
            var numeros = await _context.Colectivos
                .AsNoTracking()
                .Select(c => c.NroColectivo)
                .ToListAsync();

            return numeros
                .Where(n => !string.IsNullOrWhiteSpace(n) && ColectivoService.ObtenerNumeroLiberado(n) == null)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<int, List<object>>> ObtenerMontajesActivosAsync(IEnumerable<int> idsColectivo)
        {
            var ids = idsColectivo.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, List<object>>();
            }

            var montajes = await _context.MontajesCubierta
                .AsNoTracking()
                .Include(m => m.Cubierta)
                .Include(m => m.UbicacionCubierta)
                .Where(m => m.IdColectivo.HasValue && ids.Contains(m.IdColectivo.Value) && m.FechaDesinstalacion == null)
                .OrderBy(m => m.IdUbicacion)
                .Select(m => new
                {
                    IdColectivo = m.IdColectivo!.Value,
                    IdUbicacion = m.IdUbicacion ?? 0,
                    DescripcionUbicacion = m.UbicacionCubierta != null ? m.UbicacionCubierta.Descripcion : string.Empty,
                    NroSerie = m.Cubierta != null ? m.Cubierta.NroSerie : string.Empty,
                    EstadoCubierta = m.Cubierta != null ? m.Cubierta.Estado.ToString() : string.Empty
                })
                .ToListAsync();

            return montajes
                .GroupBy(m => m.IdColectivo)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => (object)new
                    {
                        idUbicacion = m.IdUbicacion,
                        descripcionUbicacion = m.DescripcionUbicacion,
                        nroSerie = m.NroSerie,
                        estadoCubierta = m.EstadoCubierta
                    }).ToList());
        }

        private static object MapColectivo(Colectivo colectivo, IReadOnlyDictionary<int, List<object>> montajesActivos, ISet<string> numerosAsignados)
        {
            var sinAsignacion = ColectivoService.EsSinAsignacion(colectivo);
            var tieneNumeroVacante = ColectivoService.TieneNumeroVacante(colectivo);
            var numeroLiberado = ColectivoService.ObtenerNumeroLiberado(colectivo.NroColectivo);
            var numeroDisponibleActual = !string.IsNullOrWhiteSpace(numeroLiberado) && !numerosAsignados.Contains(numeroLiberado)
                ? numeroLiberado
                : null;
            var ultimoCambio = colectivo.CambiosAceite?
                .OrderByDescending(ca => ca.Fecha)
                .FirstOrDefault();
            montajesActivos.TryGetValue(colectivo.IdColectivo, out var cubiertasMontadas);

            return new
            {
                idColectivo = colectivo.IdColectivo,
                nroColectivo = tieneNumeroVacante ? string.Empty : colectivo.NroColectivo,
                numeroLiberado,
                numeroDisponibleActual,
                patente = colectivo.Patente,
                modelo = colectivo.Modelo,
                estado = (int)colectivo.Estado,
                estadoDescripcion = colectivo.Estado.ToString(),
                sinAsignacion,
                kilometraje = colectivo.Kilometraje,
                vtoVTV = colectivo.VtoVTV?.ToString("yyyy-MM-dd"),
                cubiertasMontadas = cubiertasMontadas ?? new List<object>(),
                ultimoCambioAceite = ultimoCambio == null ? null : new
                {
                    kilometros = ultimoCambio.Kilometros,
                    fecha = ultimoCambio.Fecha.ToString("yyyy-MM-dd")
                }
            };
        }
    }
}
