using Microsoft.AspNetCore.Mvc;
using ApiSwagger.Models;
using webApi.Dtos;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace webApi.Controllers.Usuarios
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
    private readonly ApiSwagger.Data.AppDbContext _context;
    public UsuariosController(ApiSwagger.Data.AppDbContext context)
        {
            _context = context;
        }

        // Solo el administrador puede crear usuarios
        [HttpPost("registrar")]
    public IActionResult Registrar([FromBody] UsuarioRegistroDto usuarioDto)
        {
            // Aquí deberías validar que el usuario actual es administrador
            // ...validación de rol...

            if (_context.Usuarios.Any(u => u.NombreUsuario == usuarioDto.NombreUsuario))
                return BadRequest("El nombre de usuario ya existe");

            var usuario = new Usuario
            {
                NombreUsuario = usuarioDto.NombreUsuario,
                ContraseñaHash = HashPassword(usuarioDto.Contraseña),
                Rol = usuarioDto.Rol
            };
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("login")]
    public IActionResult Login([FromBody] UsuarioLoginDto usuarioDto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == usuarioDto.NombreUsuario);
            if (usuario == null || usuario.ContraseñaHash != HashPassword(usuarioDto.Contraseña))
                return Unauthorized("Usuario o contraseña incorrectos");

            // Aquí deberías generar y devolver un JWT con el rol
            // ...generación de token...
            return Ok(new { mensaje = "Login exitoso", rol = usuario.Rol.ToString() });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
