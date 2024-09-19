using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Agrega HttpClient y configuración SSL si es necesario
builder.Services.AddHttpClient("IgnoreSSL").ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
    };
});

// Carga la configuración de Ocelot desde el archivo ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Agrega los servicios de Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

// Configura el middleware de autenticación si lo tienes
app.UseAuthentication();
app.UseOcelot().Wait();
// Inicia el middleware de Ocelot
await app.UseOcelot();

// Ejecuta la aplicación
app.Run();
