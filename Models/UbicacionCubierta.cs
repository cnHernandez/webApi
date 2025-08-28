using System.ComponentModel.DataAnnotations;

namespace ApiSwagger.Models
{
    public class UbicacionCubierta
    {
        [Key]
        public int IdUbicacion { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public static class UbicacionCubiertaCatalogo
    {
        public static List<UbicacionCubierta> GetUbicaciones() => new List<UbicacionCubierta>
        {
            new UbicacionCubierta { IdUbicacion = 1, Descripcion = "Delantera Derecha" },
            new UbicacionCubierta { IdUbicacion = 2, Descripcion = "Delantera Izquierda" },
            new UbicacionCubierta { IdUbicacion = 3, Descripcion = "Trasera Derecha Externa" },
            new UbicacionCubierta { IdUbicacion = 4, Descripcion = "Trasera Derecha Interna" },
            new UbicacionCubierta { IdUbicacion = 5, Descripcion = "Trasera Izquierda Externa" },
            new UbicacionCubierta { IdUbicacion = 6, Descripcion = "Trasera Izquierda Interna" }
        };
    }
}
