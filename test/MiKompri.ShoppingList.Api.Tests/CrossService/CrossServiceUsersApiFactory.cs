using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiKompri.Users.Infrastructure.Persistence;

namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest.CrossService
{
    /// <summary>
    /// Factory de Users.Api para tests cross-service.
    /// Utiliza el código real de Users.Api (no un fake) con BD InMemory.
    /// El auth handler acepta el mismo esquema "Authorization: Test {sub}"
    /// que usa TestAuthHandler de ShoppingList, permitiendo que el token
    /// se reenvíe sin transformación entre ambas APIs de test.
    /// </summary>
    public sealed class CrossServiceUsersApiFactory : WebApplicationFactory<UsersApiProgram>
    {
        private readonly string _dbName = $"UsersXS_{Guid.NewGuid()}";

        public CrossServiceUsersApiFactory()
        {
            // Las variables de entorno se cargan durante WebApplication.CreateBuilder,
            // antes de que se ejecute cualquier validación de startup en Program.cs.
            Environment.SetEnvironmentVariable("Authentication__Authority", "https://test.authority.placeholder/");
            Environment.SetEnvironmentVariable("Authentication__Audience", "mikompri-users-test");
            Environment.SetEnvironmentVariable("Authentication__IdentityProvider", "test");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                var extra = new Dictionary<string, string?>
                {
                    // Placeholder para pasar la validación de startup;
                    // la autenticación real se reemplaza por CrossServiceAuthHandler.
                    ["Authentication:Authority"] = "https://test.authority.placeholder/",
                    ["Authentication:Audience"] = "mikompri-users-test",
                    ["Authentication:IdentityProvider"] = "test",
                    ["ConnectionStrings:UsersPostgreSQL"] = "not-used-inmemory"
                };
                cfg.AddInMemoryCollection(extra);
            });

            builder.ConfigureServices(services =>
            {
                // Reemplazar DbContext por InMemory
                var toRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<UsersDbContext>))
                    .ToList();
                foreach (var d in toRemove) services.Remove(d);

                services.AddDbContext<UsersDbContext>(opts =>
                    opts.UseInMemoryDatabase(_dbName));

                // Reemplazar autenticación con el mismo esquema "Test {sub}"
                // para que los tokens reenviados por ShoppingList sean aceptados.
                services
                    .AddAuthentication(opt =>
                    {
                        opt.DefaultAuthenticateScheme = CrossServiceAuthHandler.SchemeName;
                        opt.DefaultChallengeScheme = CrossServiceAuthHandler.SchemeName;
                        opt.DefaultScheme = CrossServiceAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, CrossServiceAuthHandler>(
                        CrossServiceAuthHandler.SchemeName, _ => { });
            });
        }
    }

    /// <summary>
    /// Auth handler para Users.Api en contexto cross-service.
    /// Lee sub desde "Authorization: Test {sub}" — mismo formato que
    /// el TestAuthHandler de ShoppingList para que el reenvío sea transparente.
    /// </summary>
    public sealed class CrossServiceAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TestCS";

        public CrossServiceAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));

            var raw = authHeader.ToString();
            if (!raw.StartsWith("Test ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.Fail("Invalid test auth scheme."));

            var sub = raw["Test ".Length..].Trim();
            if (string.IsNullOrEmpty(sub))
                return Task.FromResult(AuthenticateResult.Fail("Empty sub."));

            var claims = new[]
            {
                new Claim("sub", sub),
                new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, sub),
                new Claim("name", sub)
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
