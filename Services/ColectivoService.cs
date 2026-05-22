using ApiSwagger.Data;
using ApiSwagger.Dtos;
using ApiSwagger.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSwagger.Services
{
    public class ColectivoService
    {
        private const string NroSinAsignacionPrefix = "__SIN_ASIGNACION__";
        private readonly AppDbContext _context;

        public ColectivoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Colectivo>> ListarAsync()
        {
            return await _context.Colectivos
                .Include(c => c.CambiosAceite)
                .OrderBy(c => c.NroColectivo)
                .ToListAsync();
        }

        public Task<Colectivo?> ObtenerPorIdAsync(int id)
        {
            return _context.Colectivos.FirstOrDefaultAsync(c => c.IdColectivo == id);
        }

        public Task<Colectivo?> ObtenerPorNumeroAsync(string nroColectivo)
        {
            var nroNormalizado = NormalizarTexto(nroColectivo);
            return _context.Colectivos.FirstOrDefaultAsync(c => c.NroColectivo == nroNormalizado);
        }

        public async Task<Colectivo> CrearAsync(GuardarColectivoDto dto)
        {
            var nroColectivo = NormalizarTexto(dto.NroColectivo);
            var existente = await _context.Colectivos.FirstOrDefaultAsync(c => c.NroColectivo == nroColectivo);

            if (existente != null)
            {
                if (!EsSinAsignacion(existente))
                {
                    throw new InvalidOperationException("Ya existe una unidad con ese número de colectivo.");
                }

                AplicarDatosAsignados(existente, dto);
                existente.Estado = EstadoColectivo.Activo;
                await _context.SaveChangesAsync();
                return existente;
            }

            var colectivo = new Colectivo
            {
                NroColectivo = nroColectivo
            };

            AplicarDatosAsignados(colectivo, dto);
            _context.Colectivos.Add(colectivo);
            await _context.SaveChangesAsync();
            return colectivo;
        }

        public async Task<Colectivo> ActualizarAsync(int id, GuardarColectivoDto dto)
        {
            var colectivo = await ObtenerRequeridoAsync(id);
            var eraSinAsignacion = EsSinAsignacion(colectivo);
            var nroColectivo = NormalizarTexto(dto.NroColectivo);

            if (!EsSinAsignacion(colectivo) && string.IsNullOrWhiteSpace(nroColectivo))
            {
                throw new InvalidOperationException("El número de colectivo es obligatorio.");
            }

            if (!string.Equals(colectivo.NroColectivo, nroColectivo, StringComparison.Ordinal))
            {
                await ReasignarNumeroAsync(colectivo, nroColectivo);
            }

            AplicarDatosAsignados(colectivo, dto);

            if (eraSinAsignacion && !string.IsNullOrWhiteSpace(nroColectivo))
            {
                colectivo.Estado = EstadoColectivo.Activo;
            }

            await _context.SaveChangesAsync();
            return colectivo;
        }

        public async Task<Colectivo> DarDeBajaAsync(int id)
        {
            var colectivo = await ObtenerRequeridoAsync(id);

            var montajes = await _context.MontajesCubierta
                .Where(m => m.IdColectivo == id && m.FechaDesinstalacion == null)
                .ToListAsync();

            foreach (var montaje in montajes)
            {
                montaje.FechaDesinstalacion = DateTime.Now;
                montaje.MotivoCambio = "BAJA COLECTIVO";

                var cubierta = await _context.Cubiertas.FindAsync(montaje.IdCubierta);
                if (cubierta == null)
                {
                    continue;
                }

                cubierta.IdColectivo = null;
                cubierta.Ubicacion = null;

                try
                {
                    var prop = _context.Entry(cubierta).Property("IdUbicacion");
                    prop.CurrentValue = null;
                }
                catch
                {
                }

                var propDesc = cubierta.GetType().GetProperty("UbicacionDescripcion");
                if (propDesc != null && propDesc.CanWrite)
                {
                    propDesc.SetValue(cubierta, string.Empty);
                }

                try
                {
                    _context.Entry(cubierta).Property("UbicacionIdUbicacion").CurrentValue = null;
                }
                catch
                {
                }
            }

            colectivo.NroColectivo = GenerarNumeroSinAsignacion(colectivo.NroColectivo);
            colectivo.Estado = EstadoColectivo.DarDeBaja;
            await _context.SaveChangesAsync();
            return colectivo;
        }

        public static bool EsSinAsignacion(Colectivo colectivo)
        {
            return colectivo.Estado == EstadoColectivo.FueraDeServicio &&
                EsNumeroInternoSinAsignacion(colectivo.NroColectivo);
        }

        public static bool TieneNumeroVacante(Colectivo colectivo)
        {
            return EsNumeroInternoSinAsignacion(colectivo.NroColectivo);
        }

        public static string? ObtenerNumeroLiberado(string? nroColectivo)
        {
            var valor = NormalizarTexto(nroColectivo);
            if (!EsNumeroInternoSinAsignacion(valor))
            {
                return null;
            }

            var numeroOriginal = valor;

            while (EsNumeroInternoSinAsignacion(numeroOriginal))
            {
                numeroOriginal = numeroOriginal.Substring(NroSinAsignacionPrefix.Length).TrimStart('-', '_', ' ');
            }

            return string.IsNullOrWhiteSpace(numeroOriginal) ? null : numeroOriginal;
        }

        private async Task<Colectivo> ObtenerRequeridoAsync(int id)
        {
            var colectivo = await ObtenerPorIdAsync(id);
            if (colectivo == null)
            {
                throw new KeyNotFoundException("No se encontró el colectivo solicitado.");
            }

            return colectivo;
        }

        private async Task ReasignarNumeroAsync(Colectivo colectivo, string nroColectivoDestino)
        {
            if (string.IsNullOrWhiteSpace(nroColectivoDestino))
            {
                throw new InvalidOperationException("El número de colectivo es obligatorio.");
            }

            var numeroQueQuedaLibre = ObtenerNumeroLiberado(colectivo.NroColectivo) ?? NormalizarTexto(colectivo.NroColectivo);

            var existente = await _context.Colectivos.FirstOrDefaultAsync(c =>
                c.NroColectivo == nroColectivoDestino && c.IdColectivo != colectivo.IdColectivo);

            if (existente == null)
            {
                colectivo.NroColectivo = nroColectivoDestino;
                return;
            }

            if (EsSinAsignacion(existente))
            {
                _context.Colectivos.Remove(existente);
                colectivo.NroColectivo = nroColectivoDestino;
                return;
            }

            existente.NroColectivo = GenerarNumeroSinAsignacion(numeroQueQuedaLibre);
            existente.Estado = EstadoColectivo.FueraDeServicio;
            colectivo.NroColectivo = nroColectivoDestino;
        }

        private static void AplicarDatosAsignados(Colectivo colectivo, GuardarColectivoDto dto)
        {
            colectivo.NroColectivo = NormalizarTexto(dto.NroColectivo);
            colectivo.Patente = NormalizarTexto(dto.Patente);
            colectivo.Modelo = string.IsNullOrWhiteSpace(dto.Modelo) ? null : dto.Modelo.Trim();
            colectivo.Estado = dto.Estado;
            colectivo.VtoVTV = dto.VtoVTV;
            colectivo.Kilometraje = dto.Kilometraje;
        }

        private static bool EsNumeroInternoSinAsignacion(string? nroColectivo)
        {
            return NormalizarTexto(nroColectivo).StartsWith(NroSinAsignacionPrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerarNumeroSinAsignacion(string? nroAnterior)
        {
            var numeroOriginal = ObtenerNumeroLiberado(nroAnterior) ?? NormalizarTexto(nroAnterior);
            return string.IsNullOrWhiteSpace(numeroOriginal)
                ? NroSinAsignacionPrefix
                : $"{NroSinAsignacionPrefix}-{numeroOriginal}";
        }

        private static string NormalizarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        }
    }
}