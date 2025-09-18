namespace ApiSwagger.Dtos
{
    public class CambioAceiteDto
    {
        public int Id { get; set; }
        public int ColectivoId { get; set; }
        public DateOnly Fecha { get; set; }
    public int Kilometros { get; set; }
    public bool FiltrosCambiados { get; set; }
    // ...propiedad Observaciones eliminada...
    }
}