using Microsoft.IdentityModel.Tokens;

namespace MiKompri.Users.Api.Security;

public static class UsersJwtValidation
{
    public static TokenValidationParameters BuildTokenValidationParameters(
        string? issuer,
        string? audience,
        string? validAudiencesRaw)
    {
        var validAudiences = validAudiencesRaw?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList() ?? new List<string>();

        if (!string.IsNullOrWhiteSpace(audience) && !validAudiences.Contains(audience, StringComparer.OrdinalIgnoreCase))
        {
            validAudiences.Add(audience);
        }

        if (!string.IsNullOrWhiteSpace(audience) && audience.StartsWith("api://", StringComparison.OrdinalIgnoreCase))
        {
            var appId = audience["api://".Length..];
            if (!string.IsNullOrWhiteSpace(appId) && !validAudiences.Contains(appId, StringComparer.OrdinalIgnoreCase))
            {
                validAudiences.Add(appId);
            }
        }

        var validIssuers = BuildValidIssuers(issuer);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = validIssuers,
            ValidateAudience = true,
            ValidAudiences = validAudiences,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true
        };
    }

    private static string[] BuildValidIssuers(string? issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            return Array.Empty<string>();
        }

        var normalizedIssuer = issuer.TrimEnd('/');
        var issuers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            normalizedIssuer,
            $"{normalizedIssuer}/"
        };

        if (Uri.TryCreate(normalizedIssuer, UriKind.Absolute, out var uri))
        {
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var tenantId = segments.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                issuers.Add($"https://sts.windows.net/{tenantId}/");
            }
        }

        return issuers.ToArray();
    }
}
