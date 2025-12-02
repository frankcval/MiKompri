using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace MiKompri.ShoppingList.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Recuperamos el CorrelationId que puso el RequestLoggingMiddleware
                var correlationId = context.Items["CorrelationId"]?.ToString()
                                    ?? context.TraceIdentifier;

                // Logueamos la excepción UNA sola vez aquí
                _logger.LogError(
                    ex,
                    "Unhandled exception. CorrelationId: {CorrelationId}",
                    correlationId);

                context.Response.ContentType = "application/json";

                object body;
                HttpStatusCode statusCode;

                switch (ex)
                {
                    case FluentValidation.ValidationException ve:
                        statusCode = HttpStatusCode.BadRequest;

                        body = new
                        {
                            status = (int)statusCode,
                            error = "La petición no es válida",
                            errors = ve.Errors.Select(e => new
                            {
                                field = e.PropertyName,
                                message = e.ErrorMessage,
                                traceId = correlationId   // <- muy útil para buscar en logs
                            })
                        };
                        break;

                    case KeyNotFoundException:
                        statusCode = HttpStatusCode.NotFound;
                        body = new
                        {
                            status = (int)statusCode,
                            error = ex.Message,
                            traceId = correlationId  

                        };
                        break;

                    case InvalidOperationException:
                        statusCode = HttpStatusCode.BadRequest;
                        body = new
                        {
                            status = (int)statusCode,
                            error = ex.Message,
                            traceId = correlationId  
                        };
                        break;

                    default:
                        statusCode = HttpStatusCode.InternalServerError;
                        body = new
                        {
                            status = (int)statusCode,
                            error = "Error interno del servidor",
                            traceId = correlationId  
                        };
                        break;
                }

                context.Response.StatusCode = (int)statusCode;

                var json = JsonSerializer.Serialize(body);
                await context.Response.WriteAsync(json);
            }
        }
    }



    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
