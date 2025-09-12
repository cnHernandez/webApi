using Microsoft.EntityFrameworkCore;
using ApiSwagger.Models;
using ApiSwagger.Data;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using HealthChecks.Uris;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy => policy
            .WithOrigins("http://localhost:8080","http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();




app.UseRouting();
app.UseCors("AllowReact");

app.MapControllers();

app.Run();

