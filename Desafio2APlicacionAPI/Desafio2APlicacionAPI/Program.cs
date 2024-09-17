using Desafio2APlicacionAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Desafio2APlicacionAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            using (var scope=app.Services.CreateScope())
            {
                var services=scope.ServiceProvider;
                var context=services.GetRequiredService<ApplicationDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            
            }
            app.Run();
        }
    }
}
