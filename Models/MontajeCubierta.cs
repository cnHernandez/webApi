using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSwagger.Models
{
    public class MontajeCubierta
    {
        [Key]
        public int IdMontaje { get; set; }
    public int IdCubierta { get; set; }
    public int? IdColectivo { get; set; }
    public int? IdUbicacion { get; set; }
        public DateTime FechaMontaje { get; set; } = DateTime.Now;
        public string MotivoCambio { get; set; } = string.Empty;
        public DateTime? FechaDesinstalacion { get; set; }

        [ForeignKey("IdCubierta")]
    public required Cubierta Cubierta { get; set; }
    [ForeignKey("IdColectivo")]
    public Colectivo? Colectivo { get; set; }
    [ForeignKey("IdUbicacion")]
    public UbicacionCubierta? UbicacionCubierta { get; set; }
    }
}
