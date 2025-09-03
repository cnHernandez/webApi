using System.ComponentModel.DataAnnotations;

namespace ApiSwagger.Models
{
    public enum RolUsuario
    {
        Administrador,
        Gomeria
    }

    public class Usuario
    {
    public int Id { get; set; }
    [Required]
    public required string NombreUsuario { get; set; }
    [Required]
    public required string Contraseña { get; set; }
    [Required]
    public RolUsuario Rol { get; set; }
    }
}
