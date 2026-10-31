using Microsoft.IdentityModel.Tokens;
using MiKompri.Identity;
namespace MiKompri.ProductCatalog.Api.Security;
public static class ProductCatalogJwtValidation
{
    public static TokenValidationParameters BuildTokenValidationParameters(string? issuer, string? audience, string? validAudiencesRaw) =>
        SingleIssuerParameters.Build(issuer, audience, validAudiencesRaw);
}
