using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace MiKompri.Identity.TestSupport;
internal static class SignedAuthentication
{
    public const string LegacyTenant = "11111111-1111-1111-1111-111111111111";
    public const string CiamTenant = "22222222-2222-2222-2222-222222222222";
    public const string LegacyIssuer = "https://login.microsoftonline.com/" + LegacyTenant + "/v2.0";
    public const string CiamIssuer = "https://" + CiamTenant + ".ciamlogin.com/" + CiamTenant + "/v2.0";
    private static readonly RsaSecurityKey Key = new(RSA.Create(2048)) { KeyId = "isolated-signature-test" };
    public static void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string,string?> {
            ["Authentication:Authority"] = LegacyIssuer, ["Authentication:TenantId"] = LegacyTenant,
            ["Authentication:Audience"] = "legacy-api", ["Authentication:ValidAudiences"] = "legacy-api",
            ["Authentication:ClientIds"] = "legacy-spa", ["Authentication:ExternalId:Enabled"] = "true",
            ["Authentication:ExternalId:BusinessEnabled"] = "true", ["Authentication:ExternalId:Authority"] = CiamIssuer,
            ["Authentication:ExternalId:Issuer"] = CiamIssuer, ["Authentication:ExternalId:TenantId"] = CiamTenant,
            ["Authentication:ExternalId:Audience"] = "ciam-api", ["Authentication:ExternalId:ClientIds"] = "ciam-spa"
        }));
        builder.ConfigureServices(services => {
            services.PostConfigure<AuthenticationOptions>(options => {
                options.DefaultScheme = options.DefaultAuthenticateScheme = options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            foreach (var scheme in new[] { "MiKompri-legacy", "MiKompri-ciam" })
                services.PostConfigure<JwtBearerOptions>(scheme, options => {
                    options.Authority = options.MetadataAddress = string.Empty;
                    options.Configuration = new OpenIdConnectConfiguration { Issuer = scheme.EndsWith("ciam") ? CiamIssuer : LegacyIssuer };
                    options.Configuration.SigningKeys.Add(Key);
                    options.ConfigurationManager = new Microsoft.IdentityModel.Protocols.StaticConfigurationManager<OpenIdConnectConfiguration>(options.Configuration);
                    options.TokenValidationParameters.IssuerSigningKey = Key;
                });
        });
    }
    public static string Token(string scenario)
    {
        if (scenario == "malformed") return "not-a-jwt";
        var ciam = scenario != "legacy" && scenario != "old-new";
        var issuer = ciam ? CiamIssuer : LegacyIssuer; var tenant = ciam ? CiamTenant : LegacyTenant;
        var audience = ciam ? "ciam-api" : "legacy-api"; var scope = "access_as_user";
        var client = ciam ? "ciam-spa" : "legacy-spa";
        var oid = "33333333-3333-3333-3333-333333333333";
        var starts = DateTime.UtcNow.AddMinutes(-10);
        var expiry = DateTime.UtcNow.AddMinutes(5);
        switch (scenario)
        {
            case "future-nbf": starts = DateTime.UtcNow.AddMinutes(2); break;
            case "missing-oid": oid = ""; break;
            case "wrong-client": client = "untrusted-spa"; break;
            case "google": issuer = "https://accounts.google.com"; break;
            case "graph": audience = "00000003-0000-0000-c000-000000000000"; break;
            case "id": audience = "ciam-spa"; break;
            case "unknown-issuer": issuer = "https://attacker.invalid"; break;
            case "wrong-tenant": tenant = LegacyTenant; break;
            case "wrong-audience": audience = "other-api"; break;
            case "missing-scope": scope = ""; break;
            case "wrong-scope": scope = "other_scope"; break;
            case "expired": expiry = DateTime.UtcNow.AddMinutes(-5); break;
            case "old-new": audience = "ciam-api"; break;
            case "new-old": audience = "legacy-api"; break;
        }
        var claims = new[] { new Claim("tid", tenant), new Claim("oid", oid),
            new Claim("azp", client), new Claim("scp", scope),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) };
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, claims,
            notBefore: starts, expires: expiry, signingCredentials: scenario == "unsigned" ? null :
                scenario == "hs256" ? new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(new string('x', 64))), SecurityAlgorithms.HmacSha256) :
                new SigningCredentials(scenario == "wrong-key" ? new RsaSecurityKey(RSA.Create(2048)) : Key, SecurityAlgorithms.RsaSha256)));
    }
}
