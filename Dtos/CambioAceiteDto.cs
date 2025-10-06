namespace ApiSwagger.Dtos
{
    public class CambioAceiteDto
    {
        public int Id { get; set; }
        public int ColectivoId { get; set; }
        public DateOnly Fecha { get; set; }
        public decimal Kilometros { get; set; } // Cambiado de int a decimal
        public bool FiltrosCambiados { get; set; }
        // ...propiedad Observaciones eliminada...
    }
}