using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Agrega HttpClient y configuraci�n SSL si es necesario
builder.Services.AddHttpClient("IgnoreSSL").ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
    };
});

// Carga la configuraci�n de Ocelot desde el archivo ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Agrega los servicios de Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

// Configura el middleware de autenticaci�n si lo tienes
app.UseAuthentication();
app.UseOcelot().Wait();
// Inicia el middleware de Ocelot
await app.UseOcelot();

// Ejecuta la aplicaci�n
app.Run();
