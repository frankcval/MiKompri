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
            .ToArray();

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudiences = validAudiences is { Length: > 0 }
                ? validAudiences
                : new[] { audience ?? string.Empty },
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true
        };
    }
}
