using System.Diagnostics;

namespace MiKompri.ProductCatalog.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var cid)
            ? cid.ToString()
            : context.TraceIdentifier;

        context.Items["CorrelationId"] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationId
               }))
        {
            var request = context.Request;

            _logger.LogInformation(
                "Starting HTTP {Method} {Path}. CorrelationId: {CorrelationId}",
                request.Method,
                request.Path,
                correlationId);

            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "Finished HTTP {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}
