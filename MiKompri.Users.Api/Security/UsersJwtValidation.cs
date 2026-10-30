using Microsoft.IdentityModel.Tokens;
using MiKompri.Identity;
namespace MiKompri.Users.Api.Security;
public static class UsersJwtValidation
{
    public static TokenValidationParameters BuildTokenValidationParameters(string? issuer, string? audience, string? validAudiencesRaw) =>
        SingleIssuerParameters.Build(issuer, audience, validAudiencesRaw);
}
