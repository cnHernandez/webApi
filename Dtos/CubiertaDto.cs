using System;

namespace ApiSwagger.Dtos
{
    public class EstadoCubiertaDto
    {
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaRecapada { get; set; }
    public DateTime? FechaDobleRecapada { get; set; }
    public string MotivoCambio { get; set; } = string.Empty;
    }

    public class CubiertaDto
    {
        public int IdCubierta { get; set; }
        public string NroSerie { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Medida { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public EstadoCubiertaDto EstadoInfo { get; set; } = new EstadoCubiertaDto();
        public int IdColectivo { get; set; }
        public int IdUbicacion { get; set; }
        public string UbicacionDescripcion { get; set; } = string.Empty;
    }
}
