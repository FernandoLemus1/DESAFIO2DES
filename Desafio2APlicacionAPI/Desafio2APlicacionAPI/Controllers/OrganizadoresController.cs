using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using Desafio2APlicacionAPI.Models;

namespace Desafio2APlicacionAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class OrganizadoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public OrganizadoresController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        // GET: api/Organizadores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organizador>>> GetOrganizadores()
        {
            var db = _redis.GetDatabase();
            string cacheKey = "organizadoresList";
            var organizadoresCache = await db.StringGetAsync(cacheKey);

            if (!organizadoresCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<List<Organizador>>(organizadoresCache);
            }

            var organizadores = await _context.Organizador.ToListAsync();
            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(organizadores), TimeSpan.FromMinutes(10));
            return organizadores;
        }

        // GET: api/Organizadores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Organizador>> GetOrganizador(int id)
        {
            var db = _redis.GetDatabase();
            string cacheKey = $"organizador_{id}";
            var organizadorCache = await db.StringGetAsync(cacheKey);

            if (!organizadorCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<Organizador>(organizadorCache);
            }

            var organizador = await _context.Organizador.FindAsync(id);
            if (organizador == null)
            {
                return NotFound();
            }

            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(organizador), TimeSpan.FromMinutes(10));
            return organizador;
        }

        // PUT: api/Organizadores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrganizador(int id, Organizador organizador)
        {
            if (id != organizador.OrganizadorId)
            {
                return BadRequest();
            }

            _context.Entry(organizador).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                var db = _redis.GetDatabase();
                string cacheKey = $"organizador_{id}";
                await db.KeyDeleteAsync(cacheKey);
                await db.KeyDeleteAsync("organizadoresList");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizadorExists(id))
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

        // POST: api/Organizadores
        [HttpPost]
        public async Task<ActionResult<Organizador>> PostOrganizador(Organizador organizador)
        {
            _context.Organizador.Add(organizador);
            await _context.SaveChangesAsync();

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync("organizadoresList");

            return CreatedAtAction("GetOrganizador", new { id = organizador.OrganizadorId }, organizador);
        }

        // DELETE: api/Organizadores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganizador(int id)
        {
            var organizador = await _context.Organizador.FindAsync(id);
            if (organizador == null)
            {
                return NotFound();
            }

            _context.Organizador.Remove(organizador);
            await _context.SaveChangesAsync();

            var db = _redis.GetDatabase();
            string cacheKey = $"organizador_{id}";
            await db.KeyDeleteAsync(cacheKey);
            await db.KeyDeleteAsync("organizadoresList");

            return NoContent();
        }

        private bool OrganizadorExists(int id)
        {
            return _context.Organizador.Any(e => e.OrganizadorId == id);
        }
    }

}
