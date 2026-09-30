using System.Diagnostics;
using System.Text.Json;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Api.Middleware
{
    public class UserProvisioningMiddleware
    {
        private readonly RequestDelegate _next;

        public UserProvisioningMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserIdentityResolver userIdentityResolver)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var sub = context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(sub))
            {
                await WriteUnauthorizedAsync(context);
                return;
            }

            var userId = await userIdentityResolver.ResolveCurrentUserIdAsync(context.RequestAborted);
            if (userId is null || userId == Guid.Empty)
            {
                await WriteUnauthorizedAsync(context);
                return;
            }

            context.Items["UserId"] = userId.Value;

            await _next(context);
        }

        private static Task WriteUnauthorizedAsync(HttpContext context)
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

            return context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }
}
