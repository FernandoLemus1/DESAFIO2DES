using System.ComponentModel.DataAnnotations;

namespace Desafio2APlicacionAPI.Models
{
    public class Evento
    {
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 5)]
        public string Nombre { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        [Required, StringLength(100, MinimumLength = 5)]
        public string Lugar { get; set; }

        public ICollection<Participante> Participantes { get; set; }=new List<Participante>();
        public ICollection<Organizador> Organizadores { get; set; } = new List<Organizador>();
    }
}
