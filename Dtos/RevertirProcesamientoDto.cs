namespace ApiSwagger.Dtos
{
    public class RevertirProcesamientoDto
    {
        /// <summary>
        /// Nombres de los archivos CSV en la carpeta procesados/ de S3 que deben revertirse.
        /// Ejemplo: ["142_20260312_080000_Km_Gps.csv", "142_20260313_080000_Km_Gps.csv"]
        /// </summary>
        public List<string> Archivos { get; set; } = new();
    }
}
