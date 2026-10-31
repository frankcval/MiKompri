using System.Security.Claims;
using System.Data.Common;
using MediatR;
using MiKompri.Identity;
using MiKompri.Users.Application.Commands.SyncProfile;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Api.Middleware;

public sealed class UserProvisioningMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ISender sender, IConfiguration configuration, IUserRepository users)
    {
        if (context.User.Identity?.IsAuthenticated != true || !AccountAccess.IsProtected(context))
        { await next(context); return; }
        var tid = First(context.User, "tid", "http://schemas.microsoft.com/identity/claims/tenantid");
        var oid = First(context.User, "oid", "http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrWhiteSpace(tid) || string.IsNullOrWhiteSpace(oid))
        { await AccountAccess.DenyAsync(context, 401); return; }
        var provider = context.Items[AuthenticationExtensions.ProviderItem] as string ??
            configuration["Authentication:IdentityProvider"] ?? "entra";
        var isSync = HttpMethods.IsPost(context.Request.Method) && (context.Request.Path.Value == "/api/v1/users/me/sync");
        var statusRead = (context.Request.Path.Value == "/api/v1/users/me/access-status");
        // CIAM may bootstrap only at an intentional sync; legacy compatibility stays intact.
        var mayProvision = !statusRead && (provider == "entra" || isSync) &&
            !configuration.GetValue<bool>("Authentication:ProvisioningPaused");
        if (provider == "ciam" && !configuration.GetValue<bool>("Authentication:ExternalId:BusinessEnabled") &&
            !isSync && context.Request.Path.Value != "/api/v1/users/me")
        { await AccountAccess.DenyAsync(context, 503); return; }
        var legacyClients = AuthenticationExtensions.Split(configuration["Authentication:LegacyFallbackClientIds"]);
        var client = First(context.User, "azp", "appid");
        var allowFallback = provider == "entra" && context.Items[AuthenticationExtensions.RouteItem] is TrustRoute &&
            client is not null && legacyClients.Contains(client, StringComparer.Ordinal);
        try
        {
            var result = await sender.Send(new SyncProfileCommand(provider, tid, oid,
                First(context.User, "sub", ClaimTypes.NameIdentifier),
                First(context.User, "name", ClaimTypes.Name),
                First(context.User, "email", ClaimTypes.Email),
                AllowProvision: mayProvision, AllowLegacyFallback: allowFallback,
                GivenName: First(context.User, "given_name"), FamilyName: First(context.User, "family_name")), context.RequestAborted);
            var user = await users.GetByIdAsync(result.UserId, context.RequestAborted);
            if (user is null) { await AccountAccess.DenyAsync(context, 503); return; }
            var status = new AccessStatus(user.Id, user.IsSuspended, user.SecurityVersion, user.TokensValidFromExclusive);
            var code = AccountAccess.Evaluate(status, context.User.FindFirst("iat")?.Value);
            if (code != 200) { await AccountAccess.DenyAsync(context, code); return; }
            context.Items["UserId"] = user.Id;
            context.Items["ProfileCreatedInMiddleware"] = result.Created;
            context.Items["MiKompri.AccessStatus"] = status;
        }
        catch (UnauthorizedAccessException) { await AccountAccess.DenyAsync(context, 401); return; }
        catch (Exception ex) when (ex is DbException or TimeoutException ||
            ex is OperationCanceledException && !context.RequestAborted.IsCancellationRequested)
        { await AccountAccess.DenyAsync(context, 503); return; }
        await next(context);
    }
    private static string? First(ClaimsPrincipal user, params string[] types) =>
        types.Select(t => user.FindFirst(t)?.Value).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
