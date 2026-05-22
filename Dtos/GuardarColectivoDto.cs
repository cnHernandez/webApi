using System.ComponentModel.DataAnnotations;
using ApiSwagger.Models;

namespace ApiSwagger.Dtos
{
    public class GuardarColectivoDto
    {
        [Required]
        public string NroColectivo { get; set; } = string.Empty;

        [Required]
        public string Patente { get; set; } = string.Empty;

        public string? Modelo { get; set; }
        public EstadoColectivo Estado { get; set; } = EstadoColectivo.Activo;
        public DateOnly? VtoVTV { get; set; }
        public decimal? Kilometraje { get; set; }
    }
}