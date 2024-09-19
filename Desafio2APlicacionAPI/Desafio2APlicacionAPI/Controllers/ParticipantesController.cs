using Desafio2APlicacionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ParticipantesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer _redis;

    public ParticipantesController(AppDbContext context, IConnectionMultiplexer redis)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
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

        // Incluye la relación de evento en la consulta
        var participantes = await _context.Participante
                                          .Include(p => p.Evento)
                                          .ToListAsync();

        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(participantes), TimeSpan.FromMinutes(10));
        return participantes;
    }


    // GET: api/Participantes/5
    [HttpGet("{id}")] // Ruta específica para obtener un participante por ID
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

    // PUT: api/Participantes/5
    [HttpPut("{id}")] // Ruta específica para actualizar un participante por ID
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

    // POST: api/Participantes
    [HttpPost] // Ruta para crear un nuevo participante
    public async Task<ActionResult<Participante>> PostParticipante(Participante participante)
    {
        _context.Participante.Add(participante);
        await _context.SaveChangesAsync();

        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync("participantesList");

        return CreatedAtAction("GetParticipante", new { id = participante.ParticipanteId }, participante);
    }

    // DELETE: api/Participantes/5
    [HttpDelete("{id}")] // Ruta específica para eliminar un participante por ID
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

    private bool ParticipanteExists(int id)
    {
        return _context.Participante.Any(e => e.ParticipanteId == id);
    }
}
