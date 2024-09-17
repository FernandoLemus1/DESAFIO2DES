using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Desafio2APlicacionAPI.Models
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
        public DbSet<Evento> Evento { get; set; } = default!;
        public DbSet<Organizador> Organizador { get; set; } = default!;
        public DbSet<Participante> Participante { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
