namespace webApi.Dtos
{
    public class UsuarioDto
    {
    public required string NombreUsuario { get; set; }
    public required string Contrasena { get; set; }
    public ApiSwagger.Models.RolUsuario? Rol { get; set; }
    }
}
