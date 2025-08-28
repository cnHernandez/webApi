using System;

namespace ApiSwagger.Dtos
{
    public class MontajeCubiertaDto
    {
        public int IdCubierta { get; set; }
        public int IdColectivo { get; set; }
        public int IdUbicacion { get; set; }
        public string MotivoCambio { get; set; } = string.Empty;
    }

    public class MontajeCubiertaInfoDto
    {
        public int IdMontaje { get; set; }
        public int IdCubierta { get; set; }
        public int IdColectivo { get; set; }
        public int IdUbicacion { get; set; }
        public DateTime FechaMontaje { get; set; }
        public string MotivoCambio { get; set; } = string.Empty;
        public DateTime? FechaDesinstalacion { get; set; }
        public string NroSerieCubierta { get; set; } = string.Empty;
        public string NroColectivo { get; set; } = string.Empty;
        public string DescripcionUbicacion { get; set; } = string.Empty;
    }

    public class HistorialMontajeCubiertaDto
    {
        public string NroSerieCubierta { get; set; } = string.Empty;
        public string NroColectivo { get; set; } = string.Empty;
        public string DescripcionUbicacion { get; set; } = string.Empty;
        public DateTime FechaMontaje { get; set; }
        public string MotivoCambio { get; set; } = string.Empty;
        public DateTime? FechaDesinstalacion { get; set; }
    }
}

