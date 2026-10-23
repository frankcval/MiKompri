using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MiKompri.Users.Api.Security;

namespace MiKompri.Users.Api.Tests.Security;

public class CommonAudienceValidationTests
{
    [Fact]
    public void ValidateToken_WithConfiguredCommonAudience_ShouldSucceed()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345678901234567890123456789012"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "https://issuer.local",
            audience: "api://mikompri-backend",
            claims: new[] { new Claim("sub", "user-1") },
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        var raw = new JwtSecurityTokenHandler().WriteToken(token);

        var parameters = UsersJwtValidation.BuildTokenValidationParameters(
            issuer: "https://issuer.local",
            audience: "api://mikompri-backend",
            validAudiencesRaw: "api://mikompri-backend,mikompri-users");
        parameters.IssuerSigningKey = key;

        var principal = new JwtSecurityTokenHandler().ValidateToken(raw, parameters, out _);

        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateToken_WithInvalidAudience_ShouldFail()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345678901234567890123456789012"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "https://issuer.local",
            audience: "api://wrong-audience",
            claims: new[] { new Claim("sub", "user-1") },
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        var raw = new JwtSecurityTokenHandler().WriteToken(token);

        var parameters = UsersJwtValidation.BuildTokenValidationParameters(
            issuer: "https://issuer.local",
            audience: "api://mikompri-backend",
            validAudiencesRaw: "api://mikompri-backend,mikompri-users");
        parameters.IssuerSigningKey = key;

        Action act = () => new JwtSecurityTokenHandler().ValidateToken(raw, parameters, out _);
        Assert.Throws<SecurityTokenInvalidAudienceException>(act);
    }
}
