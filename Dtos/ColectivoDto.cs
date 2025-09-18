namespace ApiSwagger.Dtos
{
    public class ColectivoDto
    {
        public int IdColectivo { get; set; }
        public string NroColectivo { get; set; } = string.Empty;
        public string Patente { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public int? Kilometraje { get; set; }
    }
}