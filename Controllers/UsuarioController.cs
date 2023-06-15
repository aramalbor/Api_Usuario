using Api_Usuario.Data;
using Api_Usuario.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Api_Usuario.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : Controller
    {

        private readonly UsuarioContext _context;

        public UsuarioController(UsuarioContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IEnumerable<Usuario> AllUsers() 
        {
            return _context.Usuarios.OrderBy(x => x.Id);    
        }

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

        [HttpGet("clave")]
        public async Task<ActionResult<string>> UsuariosByClave(string clave)
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
            usuarios.Uid = clave;

            _context.Usuarios.Add(usuarios);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuarios.Id }, usuarios);
        }
    } 
}
