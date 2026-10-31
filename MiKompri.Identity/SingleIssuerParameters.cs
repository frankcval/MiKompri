using Microsoft.IdentityModel.Tokens;
namespace MiKompri.Identity;

// Single-issuer cryptographic parameters for existing callers. Runtime APIs use
// AddMiKompriJwt + ValidatePrincipal (canonical tuple, client and scope) together.
public static class SingleIssuerParameters
{
    public static TokenValidationParameters Build(string? issuer, string? audience, string? aliases)
    {
        var issuers = new List<string>();
        if (!string.IsNullOrWhiteSpace(issuer))
        {
            issuers.Add(issuer.TrimEnd('/')); issuers.Add(issuer.TrimEnd('/') + "/");
            if (Uri.TryCreate(issuer, UriKind.Absolute, out var uri) && uri.Host == "login.microsoftonline.com")
            {
                var tid = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (Guid.TryParse(tid, out _)) issuers.Add($"https://sts.windows.net/{tid}/");
            }
        }
        var audiences = AuthenticationExtensions.Split(aliases).ToList();
        if (!string.IsNullOrWhiteSpace(audience))
        {
            audiences.Add(audience);
            if (audience.StartsWith("api://", StringComparison.Ordinal)) audiences.Add(audience[6..]);
        }
        return new TrustRoute("single", "entra", issuer ?? "", "", issuers.Distinct().ToArray(), audiences.Distinct().ToArray(), "access_as_user", []).BuildParameters();
    }
}
