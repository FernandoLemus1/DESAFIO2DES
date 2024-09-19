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

            // Incluye las relaciones de Participantes y Organizadores en la consulta
            var eventos = await _context.Evento
                                        .Include(e => e.Participantes) // Carga los participantes
                                        .Include(e => e.Organizadores) // Carga los organizadores
                                        .ToListAsync();


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



        // POST: api/Eventos
        [HttpPost]
        public async Task<ActionResult<Evento>> PostEvento(Evento evento)
        {
            // Guardar el evento primero
            _context.Evento.Add(evento);
            await _context.SaveChangesAsync(); // EventoId se genera aquí

            // Ahora, asignar el EventoId a los participantes y organizadores
            if (evento.Participantes != null && evento.Participantes.Any())
            {
                foreach (var participante in evento.Participantes)
                {
                    participante.EventoId = evento.EventoId; // Asignar el EventoId generado
                    _context.Participante.Add(participante);
                }
            }

            if (evento.Organizadores != null && evento.Organizadores.Any())
            {
                foreach (var organizador in evento.Organizadores)
                {
                    organizador.EventoId = evento.EventoId; // Asignar el EventoId generado
                    _context.Organizador.Add(organizador);
                }
            }

            // Guardar los cambios de participantes y organizadores
            await _context.SaveChangesAsync();

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync("eventosList");

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
  

        private bool EventoExists(int id)
        {
            return _context.Evento.Any(e => e.EventoId == id);
        }
    }


}
