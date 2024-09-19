using System.ComponentModel.DataAnnotations;

namespace Desafio2APlicacionAPI.Models
{
    public class Organizador
    {
        [Key]
        public int OrganizadorId { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Nombre { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Cargo { get; set; }

        [Required]
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }
    }
}
