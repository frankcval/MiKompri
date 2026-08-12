using System.Diagnostics;
using System.Text.Json;

namespace MiKompri.ShoppingList.Api.Middleware
{
    public class UserProvisioningMiddleware
    {
        private readonly RequestDelegate _next;

        public UserProvisioningMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var sub = context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var body = new
                {
                    status = StatusCodes.Status401Unauthorized,
                    error = "Authentication failed.",
                    traceId
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(body));
                return;
            }

            context.Items["UserId"] = userId;

            await _next(context);
        }
    }
}
