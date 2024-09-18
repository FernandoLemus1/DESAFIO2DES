using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using Desafio2APlicacionAPI.Models;

namespace Desafio2APlicacionAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public ParticipantesController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }
        public ParticipantesController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
           
        }

        // GET: api/Participantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Participante>>> GetParticipantes()
        {
            var db = _redis.GetDatabase();
            string cacheKey = "participantesList";
            var participantesCache = await db.StringGetAsync(cacheKey);

            if (!participantesCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<List<Participante>>(participantesCache);
            }

            var participantes = await _context.Participante.ToListAsync();
            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(participantes), TimeSpan.FromMinutes(10));
            return participantes;
        }

        [HttpGet]
        [Route("api/ParticipantesT")] // Ruta única para GetParticipantesT
        public async Task<ActionResult<IEnumerable<Participante>>> GetParticipantesT()
        {
            var participantes = await _context.Participante.ToListAsync();
            return participantes;
        }


        // GET: api/Participantes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Participante>> GetParticipante(int id)
        {
            var db = _redis.GetDatabase();
            string cacheKey = $"participante_{id}";
            var participanteCache = await db.StringGetAsync(cacheKey);

            if (!participanteCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<Participante>(participanteCache);
            }

            var participante = await _context.Participante.FindAsync(id);
            if (participante == null)
            {
                return NotFound();
            }

            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(participante), TimeSpan.FromMinutes(10));
            return participante;
        }
        // GET: api/Participantes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Participante>> GetParticipanteT(int id)
        {
         

            var participante = await _context.Participante.FindAsync(id);
            if (participante == null)
            {
                return NotFound();
            }
            return participante;
        }
        // PUT: api/Participantes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParticipante(int id, Participante participante)
        {
            if (id != participante.ParticipanteId)
            {
                return BadRequest();
            }

            _context.Entry(participante).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                var db = _redis.GetDatabase();
                string cacheKey = $"participante_{id}";
                await db.KeyDeleteAsync(cacheKey);
                await db.KeyDeleteAsync("participantesList");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipanteExists(id))
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
        public async Task<IActionResult> PutParticipanteT(int id, Participante participante)
        {
            if (id != participante.ParticipanteId)
            {
                return BadRequest("El ID no coincide.");
            }

            // Desconecta cualquier instancia previa del contexto
            var participanteExistente = await _context.Participante.AsNoTracking().FirstOrDefaultAsync(p => p.ParticipanteId == id);
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
                if (!ParticipanteExists(id))
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


        // POST: api/Participantes
        [HttpPost]
        public async Task<ActionResult<Participante>> PostParticipante(Participante participante)
        {
            _context.Participante.Add(participante);
            await _context.SaveChangesAsync();

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync("participantesList");

            return CreatedAtAction("GetParticipante", new { id = participante.ParticipanteId }, participante);
        }
        [HttpPost]
        public async Task<ActionResult<Participante>> PostParticipanteT(Participante participante)
        {
            if (string.IsNullOrEmpty(participante.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }

            _context.Participante.Add(participante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParticipanteT), new { id = participante.ParticipanteId }, participante);
        }


        // DELETE: api/Participantes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipante(int id)
        {
            var participante = await _context.Participante.FindAsync(id);
            if (participante == null)
            {
                return NotFound();
            }

            _context.Participante.Remove(participante);
            await _context.SaveChangesAsync();

            var db = _redis.GetDatabase();
            string cacheKey = $"participante_{id}";
            await db.KeyDeleteAsync(cacheKey);
            await db.KeyDeleteAsync("participantesList");

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipanteT(int id)
        {
            var participante = await _context.Participante.FindAsync(id);
            if (participante == null)
            {
                return NotFound();
            }

            _context.Participante.Remove(participante);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParticipanteExists(int id)
        {
            return _context.Participante.Any(e => e.ParticipanteId == id);
        }
    }

}
