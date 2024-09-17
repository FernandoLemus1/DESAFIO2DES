using System.ComponentModel.DataAnnotations;

namespace Desafio2APlicacionAPI.Models
{
    public class Participante
    {
        public int Id { get; set; }
        [Required, StringLength(50, MinimumLength = 3)]
        public string Nombre { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }
    }
}
