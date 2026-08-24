using DESI.ConsultorioMedico.Paciente.Aplicacion;
using DESI.ConsultorioMedico.Paciente.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: consola en desarrollo / archivo en producción (config en appsettings).
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Inyección de dependencias (Scoped por petición).
builder.Services.AddScoped<IPacienteAplicacion, PacienteAplicacion>();
builder.Services.AddScoped<IPacienteRepository, PacienteInfraestructura>();

// CORS abierto.
builder.Services.AddCors(options =>
    options.AddPolicy("Abierta", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Rate limiting: 100 peticiones por minuto.
builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
    }));

// Controllers. Se desactiva el filtro automático de ModelState para retornar ModelResponse<T>
// en los errores de validación y manejarlos en el Controller.
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);

// OpenAPI (documento consumido por Scalar).
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Consultorio Médico API",
            Version = "v1",
            Description = "API del dominio Paciente (Fase 1)."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCors("Abierta");

app.UseRateLimiter();

app.MapOpenApi();
app.MapControllers();
app.MapScalarApiReference("/scalar/v1");

app.Run();
