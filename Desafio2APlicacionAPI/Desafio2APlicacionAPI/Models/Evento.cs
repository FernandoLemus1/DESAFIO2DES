using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Desafio2APlicacionAPI.Models
{
    public class Evento
    {
        [Key]
        public int EventoId { get; set; }

        [Required, StringLength(100, MinimumLength = 5)]
        public string Nombre { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required, StringLength(100, MinimumLength = 5)]
        public string Lugar { get; set; }
        [JsonIgnore]
        public ICollection<Participante> Participantes { get; set; }=new List<Participante>();
        [JsonIgnore]
        public ICollection<Organizador> Organizadores { get; set; } = new List<Organizador>();
    }
}
