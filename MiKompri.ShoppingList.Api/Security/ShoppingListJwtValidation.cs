using Microsoft.IdentityModel.Tokens;
using MiKompri.Identity;
namespace MiKompri.ShoppingList.Api.Security;
public static class ShoppingListJwtValidation
{
    public static TokenValidationParameters BuildTokenValidationParameters(string? issuer, string? audience, string? validAudiencesRaw) =>
        SingleIssuerParameters.Build(issuer, audience, validAudiencesRaw);
}
