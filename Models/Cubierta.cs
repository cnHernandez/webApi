using System.ComponentModel.DataAnnotations;

namespace ApiSwagger.Models
{
    public enum EstadoCubierta
    {
        Nueva,
        Recapada,
        DobleRecapada
    }

    public class Cubierta
    {
    [Key]
    public int IdCubierta { get; set; }
    public string NroSerie { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Medida { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public DateTime? FechaRecapada { get; set; }
    public DateTime? FechaDobleRecapada { get; set; }
    public EstadoCubierta Estado { get; set; } = EstadoCubierta.Nueva;
    // Relación con Colectivo
    public int? IdColectivo { get; set; }
    // Relación con UbicacionCubierta
    public UbicacionCubierta? Ubicacion { get; set; }
    }

}
