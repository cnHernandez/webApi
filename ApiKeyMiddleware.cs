using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ApiSwagger
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private const string HEADER_NAME = "x-api-key";
        private readonly string? _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _apiKey = configuration["ApiSettings:ApiKey"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Validando API Key...");
            _logger.LogInformation("Clave API esperada: {ApiKey}", _apiKey);

            // Permitir preflight OPTIONS sin API key
            if (context.Request.Method == HttpMethods.Options)
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(HEADER_NAME, out var extractedApiKey))
            {
                _logger.LogWarning("Encabezado {HeaderName} no encontrado en la solicitud.", HEADER_NAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key inválida o ausente");
                return;
            }

            // Convertir extractedApiKey a cadena antes de registrar
            _logger.LogInformation("Clave API recibida: {ExtractedApiKey}", extractedApiKey.ToString() ?? "<null>");

            // Registrar todos los encabezados recibidos para depuración
            _logger.LogInformation("Encabezados recibidos: {Headers}", context.Request.Headers);

            if (string.IsNullOrEmpty(_apiKey) || extractedApiKey != _apiKey)
            {
                _logger.LogWarning("Clave API inválida. Esperada: {ApiKey}, Recibida: {ExtractedApiKey}", _apiKey, extractedApiKey);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key inválida o ausente");
                return;
            }

            await _next(context);
        }
    }
}
