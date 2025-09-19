using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSwagger.Models
{
    public class HistorialVtv
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdColectivo { get; set; }

        [ForeignKey("IdColectivo")]
        public Colectivo? Colectivo { get; set; }

        [Required]
        public DateTime FechaRealizacion { get; set; }

        [Required]
        public DateTime FechaVencimiento { get; set; }
    }
}
