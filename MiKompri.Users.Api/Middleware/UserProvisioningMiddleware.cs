using System.Diagnostics;
using System.Text.Json;
using MediatR;
using MiKompri.Users.Application.Commands.SyncProfile;

namespace MiKompri.Users.Api.Middleware
{
    public class UserProvisioningMiddleware
    {
        private readonly RequestDelegate _next;

        public UserProvisioningMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ISender sender, IConfiguration configuration)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var sub = context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(sub))
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

            var identityProvider = configuration["Authentication:IdentityProvider"] ?? "entra";
            var name = context.User.FindFirst("name")?.Value;
            var email = context.User.FindFirst("email")?.Value;

            var result = await sender.Send(
                new SyncProfileCommand(identityProvider, sub, name, email),
                context.RequestAborted);

            context.Items["UserId"] = result.UserId;
            context.Items["ProfileCreatedInMiddleware"] = result.Created;

            await _next(context);
        }
    }
}
