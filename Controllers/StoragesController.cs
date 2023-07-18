using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_Usuario.Data;
using Api_Usuario.Models;

namespace Api_Usuario.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoragesController : ControllerBase
    {
        private readonly UsuarioContext _context;

        public StoragesController(UsuarioContext context)
        {
            _context = context;
        }

        // GET: api/Storages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Storage>>> GetStorage()
        {
          if (_context.Storage == null)
          {
              return NotFound();
          }
            return await _context.Storage.ToListAsync();
        }

        // GET: api/Storages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Storage>> GetStorage(int id)
        {
          if (_context.Storage == null)
          {
              return NotFound();
          }
            var storage = await _context.Storage.FindAsync(id);

            if (storage == null)
            {
                return NotFound();
            }

            return storage;
        }

        // PUT: api/Storages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStorage(int id, Storage storage)
        {
            if (id != storage.IdStorage)
            {
                return BadRequest();
            }

            _context.Entry(storage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StorageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Storages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Storage>> PostStorage(Storage storage)
        {
          if (_context.Storage == null)
          {
              return Problem("Entity set 'UsuarioContext.Storage'  is null.");
          }
            _context.Storage.Add(storage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStorage", new { id = storage.IdStorage }, storage);
        }

        // DELETE: api/Storages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStorage(int id)
        {
            if (_context.Storage == null)
            {
                return NotFound();
            }
            var storage = await _context.Storage.FindAsync(id);
            if (storage == null)
            {
                return NotFound();
            }

            _context.Storage.Remove(storage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Route("GetByFecha")]
        public async Task<IActionResult> GetByFecha([FromBody]string id) 
        {
            if (_context.Storage == null)
            {
                return NotFound();
            }
            DateTime fechaactual = DateTime.Now;

            var storage = from s in _context.Storage
                          join u in _context.Usuarios on s.UidUser equals u.Uid
                          where u.Uid == id && fechaactual >= s.Fecha 
                          select new
                          {
                              IdStorage = s.IdStorage,
                              Grabacion = s.UrlArchivo,
                              Titulo = s.Titulo,
                              SubtituloOriginal = s.SubtituloOrginal,
                              Subtitulo = s.Subtitulo,
                              Repeticiones = s.Repeticiones,
                          };

            if (storage == null)
            {
                return NotFound();
            }


            return Ok(storage);
        }

        [HttpPut]
        [Route("UpdateStorage")]
        public async Task<IActionResult> UpStorage(UpdateStorage info) 
        {
            var storage = await _context.Storage.FindAsync(info.Id);
            if (storage == null)
            {
                return BadRequest();
            }
            DateTime tiempo = DateTime.Now;

            if (info.Intervalo.Length == 2)
            {
                int cantidad = Int32.Parse(info.Intervalo.Substring(0, 1));
                if (info.Intervalo.Substring(1).ToUpper() == "D")
                {
                    tiempo = tiempo.AddDays(cantidad);
                }
                else
                {
                    if (info.Intervalo.Substring(1).ToUpper() == "H")
                    {
                        tiempo = tiempo.AddHours(cantidad);
                    }
                }
            }
            else
            {
                if (info.Intervalo.Length == 3)
                {
                    int cantidad = Int32.Parse(info.Intervalo.Substring(0, 2));
                    if (info.Intervalo.Substring(2).ToUpper() == "D")
                    {
                        tiempo = tiempo.AddDays(cantidad);
                    }
                    else
                    {
                        if (info.Intervalo.Substring(2).ToUpper() == "H")
                        {
                            tiempo = tiempo.AddHours(cantidad);
                        }
                    }
                }
            }

            storage.Repeticiones = info.Repeticiones;
            storage.Fecha = tiempo;

            _context.Entry(storage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StorageExists(info.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

           
        }

        private bool StorageExists(int id)
        {
            return (_context.Storage?.Any(e => e.IdStorage == id)).GetValueOrDefault();
        }
    }
}
