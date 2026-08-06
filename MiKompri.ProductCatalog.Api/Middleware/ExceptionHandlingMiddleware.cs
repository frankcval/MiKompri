using System.Net;
using System.Text.Json;
using FluentValidation;
using MiKompri.ProductCatalog.Domain.Exceptions;

namespace MiKompri.ProductCatalog.Api.Middleware;

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
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;

            _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

            context.Response.ContentType = "application/json";

            object body;
            HttpStatusCode statusCode;

            switch (ex)
            {
                case ConflictException:
                    statusCode = HttpStatusCode.Conflict;
                    body = new
                    {
                        status = (int)statusCode,
                        error = ex.Message,
                        traceId = correlationId
                    };
                    break;
                case DomainException:
                    statusCode = HttpStatusCode.BadRequest;
                    body = new
                    {
                        status = (int)statusCode,
                        error = ex.Message,
                        traceId = correlationId
                    };
                    break;
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    body = new
                    {
                        status = (int)statusCode,
                        error = "La petición no es válida",
                        traceId = correlationId,
                        errors = validationException.Errors.Select(e => new
                        {
                            field = e.PropertyName,
                            message = e.ErrorMessage
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
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
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
