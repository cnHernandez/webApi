using System.ComponentModel.DataAnnotations;

namespace ApiSwagger.Models
{
    public enum EstadoColectivo
    {
        Activo,
        FueraDeServicio
    }

    public class Colectivo
    {
        [Key]
        public int IdColectivo { get; set; }
        public string NroColectivo { get; set; } = string.Empty;
        public string Patente { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public EstadoColectivo Estado { get; set; } = EstadoColectivo.Activo;
        public DateOnly? VtoVTV { get; set; }
        public int? Kilometraje { get; set; }
        // Relación con cambios de aceite
        public ICollection<CambioAceite>? CambiosAceite { get; set; }
    }
}
