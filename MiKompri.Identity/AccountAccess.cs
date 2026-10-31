using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MiKompri.Identity;
public sealed record AccessStatus(
    [property: System.Text.Json.Serialization.JsonRequired] Guid UserId,
    [property: System.Text.Json.Serialization.JsonRequired] bool IsSuspended,
    [property: System.Text.Json.Serialization.JsonRequired] long SecurityVersion,
    [property: System.Text.Json.Serialization.JsonRequired] long? TokensValidFromExclusive);

public static class AccountAccess
{
    public static bool IsProtected(HttpContext context) => context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is null &&
        (context.GetEndpoint()?.Metadata.GetOrderedMetadata<IAuthorizeData>().Count > 0);
    public static int Evaluate(AccessStatus status, string? issuedAt)
    {
        if (status.UserId == Guid.Empty || status.SecurityVersion < 0) return 503;
        if (status.IsSuspended) return 403;
        if (status.TokensValidFromExclusive is not null &&
            (!long.TryParse(issuedAt, NumberStyles.None, CultureInfo.InvariantCulture, out var iat) || iat <= status.TokensValidFromExclusive)) return 401;
        return 200;
    }
    public static Task DenyAsync(HttpContext context, int status)
    {
        context.Response.StatusCode = status;
        context.Response.Headers.CacheControl = "no-store";
        return context.Response.WriteAsJsonAsync(new { status, error = status == 503 ? "Servicio de identidad no disponible." : status == 401 ? "Authentication failed." : "Acceso no autorizado.", traceId = context.TraceIdentifier });
    }
}

public sealed class RemoteAccountStatusMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IHttpClientFactory clients, IConfiguration config)
    {
        if (!AccountAccess.IsProtected(context) || context.User.Identity?.IsAuthenticated != true) { await next(context); return; }
        if (context.Items[AuthenticationExtensions.ProviderItem] as string == "ciam" && !config.GetValue<bool>("Authentication:ExternalId:BusinessEnabled"))
        { await AccountAccess.DenyAsync(context, 503); return; }
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me/access-status");
            // Authentication has already accepted the credential. Production registers only JWT;
            // do not reinterpret or transform it when forwarding to Users.
            if (!AuthenticationHeaderValue.TryParse(context.Request.Headers.Authorization, out var header))
            { await AccountAccess.DenyAsync(context, 401); return; }
            request.Headers.Authorization = header;
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            deadline.CancelAfter(TimeSpan.FromMilliseconds(Math.Clamp(config.GetValue<int?>("UsersApi:AccessStatusTimeoutMs") ?? 1000, 50, 5000)));
            using var response = await clients.CreateClient("UsersApi").SendAsync(request, deadline.Token);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            { await AccountAccess.DenyAsync(context, (int)response.StatusCode is 401 or 403 ? (int)response.StatusCode : 503); return; }
            var status = await response.Content.ReadFromJsonAsync<AccessStatus>(deadline.Token);
            var code = status is null ? 503 : AccountAccess.Evaluate(status, context.User.FindFirst("iat")?.Value);
            if (code != 200) { await AccountAccess.DenyAsync(context, code); return; }
            context.Items["UserId"] = status!.UserId;
            context.Items["MiKompri.AccessStatus"] = status;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidOperationException)
        {
            if (context.RequestAborted.IsCancellationRequested) throw;
            await AccountAccess.DenyAsync(context, 503); return;
        }
        await next(context);
    }
}
