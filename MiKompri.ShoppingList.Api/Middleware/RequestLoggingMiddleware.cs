using System.Diagnostics;

namespace MiKompri.ShoppingList.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
             RequestDelegate next,
             ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Leer CorrelationId del header o usar TraceIdentifier
            var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var cid)
                ? cid.ToString()
                : context.TraceIdentifier;

            // Guardarlo en el contexto para otros componentes
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
                    correlationId
                );

                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Finished HTTP {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}",
                    request.Method,
                    request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId
                );
            }
        }
    }
}
