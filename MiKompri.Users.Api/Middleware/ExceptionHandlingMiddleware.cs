using System.Net;
using System.Text.Json;
using FluentValidation;
using MiKompri.Users.Domain.Abstractions;

namespace MiKompri.Users.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
                var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;

                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

                context.Response.ContentType = "application/json";

                object body;
                HttpStatusCode statusCode;

                switch (ex)
                {
                    case ValidationException ve:
                        statusCode = HttpStatusCode.BadRequest;
                        body = new
                        {
                            status = (int)statusCode,
                            error = "La petición no es válida",
                            traceId,
                            errors = ve.Errors.Select(e => new
                            {
                                field = e.PropertyName,
                                message = e.ErrorMessage
                            })
                        };
                        break;

                    case ForbiddenOperationException:
                        statusCode = HttpStatusCode.Forbidden;
                        body = new
                        {
                            status = (int)statusCode,
                            error = ex.Message,
                            traceId
                        };
                        break;

                    case InvalidOperationException:
                        statusCode = HttpStatusCode.BadRequest;
                        body = new
                        {
                            status = (int)statusCode,
                            error = ex.Message,
                            traceId
                        };
                        break;

                    case KeyNotFoundException:
                        statusCode = HttpStatusCode.NotFound;
                        body = new
                        {
                            status = (int)statusCode,
                            error = ex.Message,
                            traceId
                        };
                        break;

                    default:
                        statusCode = HttpStatusCode.InternalServerError;
                        body = new
                        {
                            status = (int)statusCode,
                            error = "Error interno del servidor",
                            traceId
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
