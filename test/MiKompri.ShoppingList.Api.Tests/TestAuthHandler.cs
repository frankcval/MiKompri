using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest
{
    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";
        public const string TidHeaderName = "X-Test-Tid";
        public const string OidHeaderName = "X-Test-Oid";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
            }

            var raw = authHeader.ToString();
            if (!raw.StartsWith("Test ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid test auth scheme."));
            }

            var subValue = raw.Substring("Test ".Length).Trim();

            var claims = new List<Claim>
            {
                new("sub", subValue),
                new(ClaimTypes.NameIdentifier, subValue)
            };

            var tid = Request.Headers.TryGetValue(TidHeaderName, out var tidValues) && !string.IsNullOrWhiteSpace(tidValues)
                ? tidValues.ToString()
                : "tenant-test";

            var oid = Request.Headers.TryGetValue(OidHeaderName, out var oidValues) && !string.IsNullOrWhiteSpace(oidValues)
                ? oidValues.ToString()
                : subValue;

            if (!string.IsNullOrWhiteSpace(tid))
                claims.Add(new Claim("tid", tid));

            if (!string.IsNullOrWhiteSpace(oid))
                claims.Add(new Claim("oid", oid));

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
