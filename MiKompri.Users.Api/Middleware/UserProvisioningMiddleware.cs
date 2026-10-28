using System.Security.Claims;
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

            var tid = FirstClaimValue(context.User, "tid", "http://schemas.microsoft.com/identity/claims/tenantid");
            var oid = FirstClaimValue(
                context.User,
                "oid",
                "http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (string.IsNullOrWhiteSpace(tid) || string.IsNullOrWhiteSpace(oid))
            {
                throw new UnauthorizedAccessException("Authentication failed.");
            }

            var sub = FirstClaimValue(context.User, "sub", ClaimTypes.NameIdentifier);
            var identityProvider = configuration["Authentication:IdentityProvider"] ?? "entra";
            var name = FirstClaimValue(context.User, "name", ClaimTypes.Name);
            var email = FirstClaimValue(context.User, "email", ClaimTypes.Email, "preferred_username", "upn");

            var result = await sender.Send(
                new SyncProfileCommand(identityProvider, tid, oid, sub, name, email),
                context.RequestAborted);

            context.Items["UserId"] = result.UserId;
            context.Items["ProfileCreatedInMiddleware"] = result.Created;

            await _next(context);
        }

        private static string? FirstClaimValue(ClaimsPrincipal user, params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var value = user.FindFirst(claimType)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

    }
}
