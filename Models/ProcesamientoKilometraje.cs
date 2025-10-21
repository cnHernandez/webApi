using System.ComponentModel.DataAnnotations;

namespace ApiSwagger.Models
{
    public class ProcesamientoKilometraje
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime FechaUltimoArchivo { get; set; }
        
        public string? NombreUltimoArchivo { get; set; }
        
        public DateTime FechaProcesamiento { get; set; }
        
        public int ArchivosProceados { get; set; }
        
        public int ColectivosActualizados { get; set; }
    }
}