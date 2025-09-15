using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ApiSwagger.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException unauthorizedEx)
            {
                // HTTP401
                var response = new
                {
                    message = "Acceso no autorizado.",
                    error = unauthorizedEx.Message
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex) 
            {
                //HTTP500
                var response = new
                {
                    message = ex.Message,
                    error = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
