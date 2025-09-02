using Microsoft.EntityFrameworkCore;
using ApiSwagger.Models;

namespace ApiSwagger.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Colectivo> Colectivos { get; set; }
    public DbSet<Cubierta> Cubiertas { get; set; }
    public DbSet<UbicacionCubierta> UbicacionesCubierta { get; set; }
    public DbSet<MontajeCubierta> MontajesCubierta { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UbicacionCubierta>().HasData(
                new UbicacionCubierta { IdUbicacion = 1, Descripcion = "Delantera Derecha" },
                new UbicacionCubierta { IdUbicacion = 2, Descripcion = "Delantera Izquierda" },
                new UbicacionCubierta { IdUbicacion = 3, Descripcion = "Trasera Derecha Externa" },
                new UbicacionCubierta { IdUbicacion = 4, Descripcion = "Trasera Derecha Interna" },
                new UbicacionCubierta { IdUbicacion = 5, Descripcion = "Trasera Izquierda Externa" },
                new UbicacionCubierta { IdUbicacion = 6, Descripcion = "Trasera Izquierda Interna" }
            );
        }
    }
}
