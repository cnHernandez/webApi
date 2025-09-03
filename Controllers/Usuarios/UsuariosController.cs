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
                Contraseña = usuarioDto.Contraseña,
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
            if (usuario == null || usuario.Contraseña != usuarioDto.Contraseña)
                return Unauthorized("Usuario o contraseña incorrectos");

            // Aquí deberías generar y devolver un JWT con el rol
            // ...generación de token...
            return Ok(new { mensaje = "Login exitoso", rol = usuario.Rol.ToString() });
        }

    // Listado de usuarios (con contraseña en texto plano)
    [HttpGet("listado")]
    public IActionResult ListadoUsuarios()
    {
        var usuarios = _context.Usuarios.ToList();
        var usuariosListado = usuarios.Select(u => new {
            u.Id,
            u.NombreUsuario,
            Contraseña = u.Contraseña,
            u.Rol
        });
        return Ok(usuariosListado);
    }

    // Baja de usuario
    [HttpDelete("baja/{id}")]
    public IActionResult BajaUsuario(int id)
    {
        var usuario = _context.Usuarios.Find(id);
        if (usuario == null)
            return NotFound("Usuario no encontrado");
        _context.Usuarios.Remove(usuario);
        _context.SaveChanges();
        return Ok("Usuario eliminado correctamente");
    }

    // Modificación de usuario
    [HttpPut("modificar/{id}")]
    public IActionResult ModificarUsuario(int id, [FromBody] UsuarioRegistroDto usuarioDto)
    {
        var usuario = _context.Usuarios.Find(id);
        if (usuario == null)
            return NotFound("Usuario no encontrado");
        usuario.NombreUsuario = usuarioDto.NombreUsuario;
        usuario.Contraseña = usuarioDto.Contraseña;
        usuario.Rol = usuarioDto.Rol;
        _context.SaveChanges();
        return Ok("Usuario modificado correctamente");
    }

    // ...eliminado método de hash y deshasheo...
    }
}
