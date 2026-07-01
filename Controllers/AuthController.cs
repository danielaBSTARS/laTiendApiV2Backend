using LaTiendaApi.DTOs.Auth;
using LaTiendaApi.Helpers;
using LaTiendaApi.Models;
using LaTiendaApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaTiendaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LatiendaContext _context;
        private readonly JwtService _jwtService;

        public AuthController(LatiendaContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
                return Unauthorized("Usuario no encontrado");

            if (!PasswordHelper.VerifyPassword(request.Password, usuario.PasswordHash, usuario.PasswordSalt))
                return Unauthorized("Contraseña incorrecta");

            var roles = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList();
            var token = _jwtService.GenerateToken(usuario, roles);

            return new LoginResponseDto
            {
                Token = token,
                Nombre = usuario.Nombre,
                Roles = roles
            };
        }

        [HttpPost("Registrar")]
        public async Task<ActionResult> Registrar(RegisterRequestDto request)
        {
            // Verificar si el correo ya está registrado
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { msj = "El email ya está registrado" });

            // Crear hash y salt de la contraseña
            PasswordHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Crear nuevo usuario
            var usuario = new Usuario
            {
                TipoDoc = request.TipoDoc,
                NroDoc = request.NroDoc,
                Nombre = request.Nombre,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // Asignar roles
            foreach (var rolId in request.Roles)
            {
                usuario.UsuarioRoles.Add(new UsuarioRole
                {
                    RolId = rolId,
                    Usuario = usuario
                });
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { msj = "Usuario registrado correctamente" });
        }
    }
}
