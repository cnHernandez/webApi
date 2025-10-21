namespace ApiSwagger.Dtos
{
    public class UltimoProcesamientoDto
    {
        public DateTime FechaUltimoArchivo { get; set; }
        public string? NombreUltimoArchivo { get; set; }
        public DateTime FechaProcesamiento { get; set; }
        public int ArchivosProceados { get; set; }
        public int ColectivosActualizados { get; set; }
        public string FechaUltimoArchivoFormateada => FechaUltimoArchivo.ToString("dd/MM/yyyy");
        public string FechaProcesamientoFormateada => FechaProcesamiento.ToString("dd/MM/yyyy HH:mm");
    }
}