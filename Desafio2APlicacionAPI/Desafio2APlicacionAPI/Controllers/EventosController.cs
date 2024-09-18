using Desafio2APlicacionAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace Desafio2APlicacionAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class EventosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public EventosController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }
        public EventosController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            
        }

        // GET: api/Eventos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Evento>>> GetEventos()
        {
            var db = _redis.GetDatabase();
            string cacheKey = "eventosList";
            var eventosCache = await db.StringGetAsync(cacheKey);

            if (!eventosCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<List<Evento>>(eventosCache);
            }

            var eventos = await _context.Evento.ToListAsync();
            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(eventos), TimeSpan.FromMinutes(10));
            return eventos;
        }

        // GET: api/Eventos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Evento>> GetEvento(int id)
        {
            var db = _redis.GetDatabase();
            string cacheKey = $"evento_{id}";
            var eventoCache = await db.StringGetAsync(cacheKey);

            if (!eventoCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<Evento>(eventoCache);
            }

            var evento = await _context.Evento.FindAsync(id);
            if (evento == null)
            {
                return NotFound();
            }

            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(evento), TimeSpan.FromMinutes(10));
            return evento;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Evento>> GetEventoT(int id)
        {
       
            var evento = await _context.Evento.FindAsync(id);
            if (evento == null)
            {
                return NotFound();
            }
            return evento;
        }

        // PUT: api/Eventos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvento(int id, Evento evento)
        {
            if (id != evento.EventoId)
            {
                return BadRequest();
            }

            _context.Entry(evento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                var db = _redis.GetDatabase();
                string cacheKey = $"evento_{id}";
                await db.KeyDeleteAsync(cacheKey);
                await db.KeyDeleteAsync("eventosList");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventoExists(id))
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
        public async Task<IActionResult> PutEventoT(int id, Evento participante)
        {
            if (id != participante.EventoId)
            {
                return BadRequest("El ID no coincide.");
            }

            // Desconecta cualquier instancia previa del contexto
            var participanteExistente = await _context.Participante.AsNoTracking().FirstOrDefaultAsync(p => p.EventoId == id);
            if (participanteExistente == null)
            {
                return NotFound();
            }

            // Marca la entidad como modificada
            _context.Entry(participante).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventoExists(id))
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

        // POST: api/Eventos
        [HttpPost]
        public async Task<ActionResult<Evento>> PostEvento(Evento evento)
        {
            _context.Evento.Add(evento);
            await _context.SaveChangesAsync();

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync("eventosList");

            return CreatedAtAction("GetEvento", new { id = evento.EventoId }, evento);
        }
        [HttpPost]
        public async Task<ActionResult<Evento>> PostEventoT(Evento evento)
        {
            _context.Evento.Add(evento);
            await _context.SaveChangesAsync();

          

            return CreatedAtAction("GetEvento", new { id = evento.EventoId }, evento);
        }

        // DELETE: api/Eventos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvento(int id)
        {
            var evento = await _context.Evento.FindAsync(id);
            if (evento == null)
            {
                return NotFound();
            }

            _context.Evento.Remove(evento);
            await _context.SaveChangesAsync();

            var db = _redis.GetDatabase();
            string cacheKey = $"evento_{id}";
            await db.KeyDeleteAsync(cacheKey);
            await db.KeyDeleteAsync("eventosList");

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventoT(int id)
        {
            var participante = await _context.Evento.FindAsync(id);
            if (participante == null)
            {
                return NotFound();
            }

            _context.Evento.Remove(participante);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventoExists(int id)
        {
            return _context.Evento.Any(e => e.EventoId == id);
        }
    }


}
