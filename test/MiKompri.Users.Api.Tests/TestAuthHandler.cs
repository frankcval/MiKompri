using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MiKompri.Users.Api.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SubHeaderName = "X-Test-Sub";
    public const string NameHeaderName = "X-Test-Name";
    public const string EmailHeaderName = "X-Test-Email";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!Request.Headers.TryGetValue(SubHeaderName, out var subValues))
            return Task.FromResult(AuthenticateResult.Fail("No sub claim"));

        var sub = subValues.ToString();
        if (sub is null)
            return Task.FromResult(AuthenticateResult.Fail("No sub claim"));

        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(sub))
            claims.Add(new Claim("sub", sub));

        if (Request.Headers.TryGetValue(NameHeaderName, out var nameValues) && !string.IsNullOrWhiteSpace(nameValues))
            claims.Add(new Claim("name", nameValues.ToString()));

        if (Request.Headers.TryGetValue(EmailHeaderName, out var emailValues) && !string.IsNullOrWhiteSpace(emailValues))
            claims.Add(new Claim("email", emailValues.ToString()));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
