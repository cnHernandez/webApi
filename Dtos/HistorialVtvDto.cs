using System;

namespace ApiSwagger.Dtos
{
    public class HistorialVtvDto
    {
        public int Id { get; set; }
        public int IdColectivo { get; set; }
        public DateTime FechaRealizacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
}
