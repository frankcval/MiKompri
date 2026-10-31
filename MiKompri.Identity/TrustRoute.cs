using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MiKompri.Identity;
public sealed record TrustRoute(string Name, string IdentityProvider, string Authority, string TenantId,
    string[] Issuers, string[] Audiences, string Scope, string[] ClientIds)
{
    public TokenValidationParameters BuildParameters() => new()
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ValidateIssuerSigningKey = true, RequireSignedTokens = true, RequireExpirationTime = true,
        ValidAlgorithms = [SecurityAlgorithms.RsaSha256], ClockSkew = TimeSpan.FromSeconds(30),
        ValidIssuers = Issuers, ValidAudiences = Audiences,
        // Metadata cannot silently widen these explicit per-issuer combinations.
        IssuerValidator = (issuer, _, _) => Issuers.Contains(issuer, StringComparer.Ordinal)
            ? issuer : throw new SecurityTokenInvalidIssuerException(),
        AudienceValidator = (audiences, _, _) => audiences is not null && audiences.Count() == 1 &&
            Audiences.Contains(audiences.Single(), StringComparer.Ordinal)
    };

    public void ValidatePrincipal(ClaimsPrincipal principal)
    {
        var tid = principal.FindFirst("tid")?.Value;
        var oid = principal.FindFirst("oid")?.Value;
        if (!Guid.TryParse(TenantId, out _) || tid != TenantId || !Guid.TryParse(oid, out var objectId) || objectId == Guid.Empty)
            throw new SecurityTokenValidationException("Invalid canonical identity.");
        var client = principal.FindFirst("azp")?.Value ?? principal.FindFirst("appid")?.Value;
        if (ClientIds.Length == 0 || client is null || !ClientIds.Contains(client, StringComparer.Ordinal))
            throw new SecurityTokenValidationException("Invalid client.");
        if (!(principal.FindFirst("scp")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(Scope, StringComparer.Ordinal) ?? false))
            throw new InvalidScopeException();
    }
}
public sealed class InvalidScopeException : Exception { }
