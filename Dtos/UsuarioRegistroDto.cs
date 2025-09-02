namespace webApi.Dtos
{
    public class UsuarioRegistroDto
    {
        public required string NombreUsuario { get; set; }
        public required string Contraseña { get; set; }
        public required ApiSwagger.Models.RolUsuario Rol { get; set; }
    }
}
