using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSwagger.Models
{
    public class CambioAceite
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ColectivoId { get; set; }
        [ForeignKey("ColectivoId")]
        public Colectivo? Colectivo { get; set; }
        [Required]
        public DateOnly Fecha { get; set; }
        [Required]
    public int Kilometros { get; set; }
    public bool FiltrosCambiados { get; set; }
    // ...propiedad Observaciones eliminada...
    }
}