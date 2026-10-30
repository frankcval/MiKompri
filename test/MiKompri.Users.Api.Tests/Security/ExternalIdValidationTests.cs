using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using MiKompri.Identity;

namespace MiKompri.Users.Api.Tests.Security;
public class ExternalIdValidationTests
{
    [Fact]
    public void MissingClientAllowlist_DoesNotTrustAnyCaller()
    {
        var route = new TrustRoute("legacy", "entra", LegacyIssuer, Tenant, [LegacyIssuer], ["legacy-api"], "access_as_user", []);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim("tid", Tenant), new Claim("oid", Guid.NewGuid().ToString()),
            new Claim("scp", "access_as_user"), new Claim("azp", "unlisted-client") }));
        Assert.Throws<SecurityTokenValidationException>(() => route.ValidatePrincipal(principal));
    }
    private const string Tenant = "11111111-1111-1111-1111-111111111111";
    private const string OtherTenant = "22222222-2222-2222-2222-222222222222";
    private const string LegacyIssuer = "https://login.microsoftonline.com/" + Tenant + "/v2.0";
    private const string CiamIssuer = "https://" + OtherTenant + ".ciamlogin.com/" + OtherTenant + "/v2.0";
    private static readonly RSA Rsa = RSA.Create(2048);
    private static readonly SecurityKey Key = new RsaSecurityKey(Rsa) { KeyId = "test-key" };

    [Theory]
    [InlineData("valid-legacy", true)]
    [InlineData("valid-ciam", true)]
    [InlineData("google", false)]
    [InlineData("graph", false)]
    [InlineData("id-spa", false)]
    [InlineData("unknown-issuer", false)]
    [InlineData("wrong-tenant", false)]
    [InlineData("wrong-audience", false)]
    [InlineData("scope-absent", false)]
    [InlineData("scope-wrong", false)]
    [InlineData("expired", false)]
    [InlineData("future-nbf", false)]
    [InlineData("old-issuer-new-audience", false)]
    [InlineData("new-issuer-old-audience", false)]
    [InlineData("missing-oid", false)]
    [InlineData("wrong-client", false)]
    public void SignedToken_ValidatesOnlyItsOwnTrustCombination(string scenario, bool accepted)
    {
        var legacy = new TrustRoute("legacy", "entra", LegacyIssuer, Tenant, [LegacyIssuer], ["legacy-api"], "access_as_user", ["legacy-spa"]);
        var ciam = new TrustRoute("ciam", "ciam", CiamIssuer, OtherTenant, [CiamIssuer], ["ciam-api"], "access_as_user", ["ciam-spa"]);
        var route = scenario is "valid-ciam" or "new-issuer-old-audience" ? ciam : legacy;
        var issuer = route.Issuers[0]; var audience = route.Audiences[0]; var tenant = route.TenantId;
        var scope = "access_as_user"; var client = route.ClientIds[0];
        var expires = DateTime.UtcNow.AddMinutes(5); var notBefore = DateTime.UtcNow.AddMinutes(-10);
        switch (scenario)
        {
            case "google": issuer = "https://accounts.google.com"; break;
            case "graph": audience = "00000003-0000-0000-c000-000000000000"; break;
            case "id-spa": audience = "legacy-spa"; break;
            case "unknown-issuer": issuer = "https://attacker.invalid"; break;
            case "wrong-tenant": tenant = OtherTenant; break;
            case "wrong-audience": audience = "wrong-api"; break;
            case "scope-absent": scope = ""; break;
            case "scope-wrong": scope = "read_other_api"; break;
            case "expired": expires = DateTime.UtcNow.AddMinutes(-5); break;
            case "future-nbf": notBefore = DateTime.UtcNow.AddMinutes(2); break;
            case "old-issuer-new-audience": audience = "ciam-api"; break;
            case "new-issuer-old-audience": audience = "legacy-api"; break;
            case "wrong-client": client = "untrusted-spa"; break;
        }
        var claims = new List<Claim> { new("tid", tenant), new("oid", scenario == "missing-oid" ? "" : Guid.NewGuid().ToString()), new("azp", client), new("scp", scope) };
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, claims, notBefore, expires,
            new SigningCredentials(Key, SecurityAlgorithms.RsaSha256)));
        var parameters = route.BuildParameters(); parameters.IssuerSigningKey = Key;
        var action = () => {
            var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token, parameters, out _);
            route.ValidatePrincipal(principal);
        };
        if (accepted) action(); else Assert.ThrowsAny<Exception>(action);
    }
}
