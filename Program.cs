using Microsoft.EntityFrameworkCore;
using ApiSwagger.Models;
using ApiSwagger.Data;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using HealthChecks.Uris;


var builder = WebApplication.CreateBuilder(args);

// Permitir ejecución especial para procesamiento de CSVs
if (args.Length > 0 && args[0] == "--process-csvs")
{
    var config = builder.Configuration;
    var services = new ServiceCollection();
    services.AddDbContext<ApiSwagger.Data.AppDbContext>(options =>
        options.UseMySql(
            config.GetConnectionString("DefaultConnection"),
            new MySqlServerVersion(new Version(8, 0, 0))
        )
    );
    var provider = services.BuildServiceProvider();
    var db = provider.GetRequiredService<ApiSwagger.Data.AppDbContext>();
    var csvFolder = "/app/kmColectivos";
    var service = new ApiSwagger.Services.CsvKilometrajeService(db, csvFolder);
    service.ProcesarArchivosCsv();
    Console.WriteLine("Procesamiento de archivos CSV finalizado.");
    return;
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ApiSwagger", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key debe ir en el header: x-api-key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();
// Agregar servicios de health checks
var mysqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
builder.Services.AddHealthChecks()
    .AddMySql(
        mysqlConnectionString,
        healthQuery: "SELECT 1",
        name: "mysql")
    .AddUrlGroup(
        new Uri("http://webui-ui-1"),
        name: "ui",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);

// Configurar Entity Framework Core con MySQL
builder.Services.AddDbContext<ApiSwagger.Data.AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);

// Configurar CORS para permitir cualquier origen temporalmente durante el desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

// Middleware de API Key global
app.UseMiddleware<ApiSwagger.ApiKeyMiddleware>(builder.Configuration);
// Middleware de manejo de excepciones
app.UseMiddleware<ApiSwagger.Middleware.ExceptionMiddleware>();
// Middleware de autenticación por roles (requiere que el usuario esté autenticado)
app.UseMiddleware<ApiSwagger.Middleware.AuthenticationMiddleware>();

app.UseRouting();
// Usar la política de CORS para permitir cualquier origen
app.UseCors("AllowAll");

app.MapControllers();

app.Run();

