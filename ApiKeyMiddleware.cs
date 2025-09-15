using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ApiSwagger
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

    private const string HEADER_NAME = "x-api-key";
    private readonly string? _apiKey;


        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
            _apiKey = Environment.GetEnvironmentVariable("API_KEY");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Permitir preflight OPTIONS sin API key
            if (context.Request.Method == HttpMethods.Options)
            {
                await _next(context);
                return;
            }
            if (string.IsNullOrEmpty(_apiKey) ||
                !context.Request.Headers.TryGetValue(HEADER_NAME, out var extractedApiKey) ||
                extractedApiKey != _apiKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key inválida o ausente");
                return;
            }
            await _next(context);
        }
    }
}
