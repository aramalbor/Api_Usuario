using Api_Usuario.Data;
using Api_Usuario.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api_Usuario.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : Controller
    {

   
        private readonly IConfiguration _configuration;
        private readonly UsuarioContext _context;

        public UsuarioController(UsuarioContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }
       // [Authorize]
        [HttpGet]
        public IEnumerable<Usuario> AllUsers() 
        {
            return _context.Usuarios.OrderBy(x => x.Id);    
        }
       // [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            if (_context.Usuarios == null)
            {
                return NotFound();
            }
            var usuarios = await _context.Usuarios.FindAsync(id);

            if (usuarios == null)
            {
                return NotFound();
            }

            return usuarios;
        }
       // [Authorize]
        [HttpPost("clave")]
        public async Task<ActionResult<string>> UsuariosByClave([FromForm]string clave)
        {
            if (_context.Usuarios == null)
            {
                return NotFound();
            }
            var usuarios = _context.Usuarios.FirstOrDefault(x => x.ClaveUsuario == clave);

            if (usuarios == null)
            {
                return NotFound();
            }

            return usuarios.Region;
        }
        // [Authorize]
        [HttpPost]
        [Route("PostUsuarios")]
        public async Task<ActionResult<Usuario>> PostUsuarios(Usuario usuarios)
        {
            if (_context.Usuarios == null)
            {
                return Problem("Entity set 'UsuariosContext.Usuarios'  is null.");
            }

            byte[] key = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            string clave = Convert.ToBase64String(key);

            if (usuarios.Uid != "string")
            {
                if (_context.Usuarios.Where(u => u.Uid == usuarios.Uid) != null)
                {
                    _context.Usuarios.Add(usuarios);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                usuarios.Uid = clave;
                _context.Usuarios.Add(usuarios);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("GetUsuario", new { id = usuarios.Id }, usuarios);
        }

        [HttpPost]
        [Route("Auth")]
        public async Task<ActionResult> Auth([FromForm]string clave)
        {
            if (_context.Usuarios == null)
            {
                return Problem("Entity set 'UsuariosContext.Usuarios'  is null.");
            }
            var user = _context.Usuarios.Where(c => c.Uid == clave);
            if(user.Count()== 1)
            {
                var token = GenerateToken(user.First().Uid);
                return Ok(new { Usuario = user, Token = token });
            }
            else
            {
                return Unauthorized();
            }

           
        }
        private string GenerateToken(string uid)
        {
            var secretKey = _configuration.GetValue<string>("JwtSettings:SecretKey");
            var issuer = _configuration.GetValue<string>("JwtSettings:Issuer");
            var audience = _configuration.GetValue<string>("JwtSettings:Audience");
            var expirationInMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, uid),
                new Claim(JwtRegisteredClaimNames.Sub, uid),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
       // [Authorize]
        [HttpGet("getSession")]
        public IActionResult Protected()
        {
            // Este endpoint está protegido por la autenticación JWT
            // Solo los usuarios autenticados pueden acceder aquí
            var username = User.Identity?.Name;

            var dataSession = getUser(username);

            return Ok(dataSession);
        }

        private object getUser(string username)
        {
            return _context.Usuarios.Where(c => c.Uid == username);
        }


        [HttpPost("GetUserID")]
        public async Task<ActionResult<string>> GetUserID([FromForm]string clave)
        { 
            var user = _context.Usuarios.FirstOrDefault(u => u.ClaveUsuario == clave);

            return user.Uid;
        }
    }
}
