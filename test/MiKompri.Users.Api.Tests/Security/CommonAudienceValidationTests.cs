using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using MiKompri.Users.Api.Security;

namespace MiKompri.Users.Api.Tests.Security;

public class CommonAudienceValidationTests
{
    private const string Audience = "api://36adb79e-9563-4642-8bc9-455358d76273";
    private const string Issuer = "https://login.microsoftonline.com/a6b72a33-ade4-45d3-a2a7-52b88d96f5eb/v2.0";

    [Fact]
    public void ValidateToken_WithApiAudienceUri_ShouldSucceed()
    {
        var key = BuildSigningKey();
        var token = BuildToken(Issuer, Audience, key);

        var parameters = BuildParameters();
        parameters.IssuerSigningKey = key;

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateToken_WithGuidAudienceAndStsIssuer_ShouldSucceed()
    {
        var key = BuildSigningKey();
        var token = BuildToken(
            "https://sts.windows.net/a6b72a33-ade4-45d3-a2a7-52b88d96f5eb/",
            "36adb79e-9563-4642-8bc9-455358d76273",
            key);

        var parameters = BuildParameters();
        parameters.IssuerSigningKey = key;

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateToken_WithInvalidAudience_ShouldFail()
    {
        var key = BuildSigningKey();
        var token = BuildToken(Issuer, "api://wrong-audience", key);

        var parameters = BuildParameters();
        parameters.IssuerSigningKey = key;

        Action act = () => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
        Assert.Throws<SecurityTokenInvalidAudienceException>(act);
    }

    private static RsaSecurityKey BuildSigningKey() => new(RSA.Create(2048)) { KeyId = "audience-test" };

    private static string BuildToken(string issuer, string audience, RsaSecurityKey key)
    {
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new[] { new Claim("sub", "user-1") },
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static TokenValidationParameters BuildParameters() =>
        UsersJwtValidation.BuildTokenValidationParameters(
            issuer: Issuer,
            audience: Audience,
            validAudiencesRaw: "api://36adb79e-9563-4642-8bc9-455358d76273,36adb79e-9563-4642-8bc9-455358d76273,mikompri-users");
}
