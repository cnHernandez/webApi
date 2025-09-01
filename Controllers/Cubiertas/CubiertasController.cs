using Microsoft.AspNetCore.Mvc;
using ApiSwagger.Data;
using ApiSwagger.Models;
using ApiSwagger.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiSwagger.Controllers.Cubiertas
{
    [ApiController]
    [Route("api/cubiertas")]
    public class CubiertasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CubiertasController(AppDbContext context)
        {
            _context = context;
        }

        // POST /cubiertas (alta)
        [HttpPost]
        public async Task<IActionResult> CrearCubierta([FromBody] CubiertaDto cubiertaDto)
        {
            // Mapear DTO a entidad
            var cubierta = new Cubierta
            {
                NroSerie = cubiertaDto.NroSerie,
                Marca = cubiertaDto.Marca,
                Medida = cubiertaDto.Medida,
                FechaCompra = cubiertaDto.FechaCompra,
                Estado = Enum.TryParse<EstadoCubierta>(cubiertaDto.EstadoInfo.Estado, true, out var estadoEnum) ? estadoEnum : EstadoCubierta.Nueva,
                FechaRecapada = cubiertaDto.EstadoInfo.FechaRecapada,
                FechaDobleRecapada = cubiertaDto.EstadoInfo.FechaDobleRecapada,
                IdColectivo = cubiertaDto.IdColectivo == 0 ? null : cubiertaDto.IdColectivo,
                Ubicacion = (cubiertaDto.IdUbicacion == 0) ? null : await _context.UbicacionesCubierta.FindAsync(cubiertaDto.IdUbicacion)
            };
            _context.Cubiertas.Add(cubierta);
            await _context.SaveChangesAsync();
            // Mapear entidad a DTO para la respuesta
            var dto = new CubiertaDto
            {
                IdCubierta = cubierta.IdCubierta,
                NroSerie = cubierta.NroSerie,
                Marca = cubierta.Marca,
                Medida = cubierta.Medida,
                FechaCompra = cubierta.FechaCompra,
                EstadoInfo = new EstadoCubiertaDto
                {
                    Estado = cubierta.Estado.ToString(),
                    FechaRecapada = cubierta.FechaRecapada,
                    FechaDobleRecapada = cubierta.FechaDobleRecapada
                },
                IdColectivo = cubierta.IdColectivo ?? 0,
                IdUbicacion = cubierta.Ubicacion?.IdUbicacion ?? 0,
                UbicacionDescripcion = cubierta.Ubicacion?.Descripcion ?? string.Empty
            };
            return CreatedAtAction(nameof(GetCubierta), new { id = cubierta.IdCubierta }, dto);
    // ...existing code...
        }

        // GET /cubiertas (stock actual)
        [HttpGet]
        public async Task<IActionResult> GetCubiertas()
        {
            var cubiertas = await _context.Cubiertas.Include(c => c.Ubicacion).ToListAsync();
            var result = new List<CubiertaDto>();
            foreach (var cubierta in cubiertas)
            {
                var dto = new CubiertaDto
                {
                    IdCubierta = cubierta.IdCubierta,
                    NroSerie = cubierta.NroSerie,
                    Marca = cubierta.Marca,
                    Medida = cubierta.Medida,
                    FechaCompra = cubierta.FechaCompra,
                    EstadoInfo = new EstadoCubiertaDto
                    {
                        Estado = cubierta.Estado.ToString(),
                        FechaRecapada = cubierta.FechaRecapada,
                        FechaDobleRecapada = cubierta.FechaDobleRecapada
                    },
                        FechaRecapada = cubierta.FechaRecapada,
                        FechaDobleRecapada = cubierta.FechaDobleRecapada,
                        FechaReparacion = cubierta.FechaReparacion,
                    IdColectivo = cubierta.IdColectivo ?? 0,
                    IdUbicacion = cubierta.Ubicacion?.IdUbicacion ?? 0,
                    UbicacionDescripcion = cubierta.Ubicacion?.Descripcion ?? string.Empty
                };
                result.Add(dto);
            }
            return Ok(result);
        }
     
        // GET /cubiertas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCubierta(int id)
        {
            var cubierta = await _context.Cubiertas.FindAsync(id);
            if (cubierta == null) return NotFound();
            var dto = new CubiertaDto
            {
                IdCubierta = cubierta.IdCubierta,
                NroSerie = cubierta.NroSerie,
                Marca = cubierta.Marca,
                Medida = cubierta.Medida,
                FechaCompra = cubierta.FechaCompra,
                EstadoInfo = new EstadoCubiertaDto
                {
                    Estado = cubierta.Estado.ToString(),
                    FechaRecapada = cubierta.FechaRecapada,
                    FechaDobleRecapada = cubierta.FechaDobleRecapada
                },
                    FechaRecapada = cubierta.FechaRecapada,
                    FechaDobleRecapada = cubierta.FechaDobleRecapada,
                    FechaReparacion = cubierta.FechaReparacion,
                IdColectivo = cubierta.IdColectivo ?? 0,
                IdUbicacion = cubierta.Ubicacion?.IdUbicacion ?? 0,
                UbicacionDescripcion = cubierta.Ubicacion?.Descripcion ?? string.Empty
            };
            return Ok(dto);
        }

        // GET /cubiertas/nroserie/{nroSerie}
        [HttpGet("nroserie/{nroSerie}")]
        public async Task<IActionResult> GetCubiertaPorNroSerie(string nroSerie)
        {
            var cubierta = await _context.Cubiertas.FirstOrDefaultAsync(c => c.NroSerie == nroSerie);
            if (cubierta == null) return NotFound();
            var dto = new CubiertaDto
            {
                IdCubierta = cubierta.IdCubierta,
                NroSerie = cubierta.NroSerie,
                Marca = cubierta.Marca,
                Medida = cubierta.Medida,
                FechaCompra = cubierta.FechaCompra,
                EstadoInfo = new EstadoCubiertaDto
                {
                    Estado = cubierta.Estado.ToString(),
                    FechaRecapada = cubierta.FechaRecapada,
                    FechaDobleRecapada = cubierta.FechaDobleRecapada
                },
                    FechaRecapada = cubierta.FechaRecapada,
                    FechaDobleRecapada = cubierta.FechaDobleRecapada,
                    FechaReparacion = cubierta.FechaReparacion,
                IdColectivo = cubierta.IdColectivo ?? 0,
                IdUbicacion = cubierta.Ubicacion?.IdUbicacion ?? 0,
                UbicacionDescripcion = cubierta.Ubicacion?.Descripcion ?? string.Empty
            };
            return Ok(dto);
        }

        // PUT /cubiertas/nroserie/{nroSerie}/estado (actualizar estado por nro de serie)
      [HttpPut("nroserie/{nroSerie}/estado")]
public async Task<IActionResult> ActualizarEstadoCubierta(string nroSerie, [FromBody] EstadoCubiertaDto body)
{
    var cubierta = await _context.Cubiertas.FirstOrDefaultAsync(c => c.NroSerie == nroSerie);
    if (cubierta == null) return NotFound();

    if (!Enum.TryParse<EstadoCubierta>(body.Estado, true, out var estadoEnum))
        return BadRequest("Estado inválido");

    cubierta.Estado = estadoEnum;

    // Actualizar la fecha según el estado
    if (estadoEnum == EstadoCubierta.Recapada && body.FechaRecapada.HasValue)
        cubierta.FechaRecapada = body.FechaRecapada.Value;
    if (estadoEnum == EstadoCubierta.DobleRecapada && body.FechaDobleRecapada.HasValue)
        cubierta.FechaDobleRecapada = body.FechaDobleRecapada.Value;

    // Si pasa a EnReparacion, desmontar la cubierta
    if (estadoEnum == EstadoCubierta.EnReparacion)
    {
        // Buscar el montaje actual sin desinstalación
        var montajeActual = await _context.MontajesCubierta
            .Where(m => m.IdCubierta == cubierta.IdCubierta && m.FechaDesinstalacion == null)
            .OrderByDescending(m => m.FechaMontaje)
            .FirstOrDefaultAsync();

        if (montajeActual != null)
        {
            montajeActual.FechaDesinstalacion = DateTime.Now;
            // Guardar el motivo del input en el montaje anterior
            montajeActual.MotivoCambio = body.MotivoCambio;
            _context.MontajesCubierta.Update(montajeActual);

            // El nuevo montaje siempre con motivo 'a Reparar'
            var nuevoMontaje = new MontajeCubierta
            {
                IdCubierta = cubierta.IdCubierta,
                IdColectivo = null,
                IdUbicacion = null,
                MotivoCambio = "a Reparar",
                FechaMontaje = DateTime.Now,
                FechaDesinstalacion = DateTime.Now,
                Cubierta = cubierta
            };
            _context.MontajesCubierta.Add(nuevoMontaje);
        }

    // Desasignar cubierta
        cubierta.IdColectivo = null;
        cubierta.Ubicacion = null;
        cubierta.FechaReparacion = DateTime.Now;
        // Desasignar también la clave foránea UbicacionIdUbicacion si existe
        if (_context.Entry(cubierta).Property("UbicacionIdUbicacion") != null)
            _context.Entry(cubierta).Property("UbicacionIdUbicacion").CurrentValue = null;
    }

    await _context.SaveChangesAsync();
    return Ok();
}
    }
}
