using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.Users.Infrastructure.Persistence;

namespace MiKompri.Users.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>
{
    private readonly string _databaseName = $"UsersApiTests-{Guid.NewGuid()}";

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Authentication__Authority", "https://login.microsoftonline.com/common/v2.0");
        Environment.SetEnvironmentVariable("Authentication__Audience", "mikompri-users");
        Environment.SetEnvironmentVariable("Authentication__IdentityProvider", "entra");
        Environment.SetEnvironmentVariable("ConnectionStrings__UsersPostgreSQL", "Host=localhost;Port=5432;Database=MiKompri_Users;Username=postgres;Password=12345");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["Authentication:Authority"] = "https://login.microsoftonline.com/common/v2.0",
                ["Authentication:Audience"] = "mikompri-users",
                ["Authentication:IdentityProvider"] = "entra",
                ["ConnectionStrings:UsersPostgreSQL"] = "Host=localhost;Port=5432;Database=MiKompri_Users;Username=postgres;Password=12345"
            };

            config.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureTestServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<UsersDbContext>) ||
                    d.ServiceType == typeof(UsersDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1" &&
                     d.ServiceType.GenericTypeArguments[0] == typeof(UsersDbContext)))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<UsersDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(inMemoryServiceProvider);
            });

            const string testScheme = "TestAuth";

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = testScheme;
                    options.DefaultChallengeScheme = testScheme;
                    options.DefaultScheme = testScheme;
                })
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                    testScheme,
                    _ => { });
        });
    }

    public HttpClient CreateAuthenticatedClient(string sub, string? name = null, string? email = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            TestAuthHandler.SubHeaderName,
            string.IsNullOrEmpty(sub) ? " " : sub);

        if (name is not null)
            client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.NameHeaderName, name);

        if (email is not null)
            client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.EmailHeaderName, email);

        return client;
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    public int CountUsers()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        return db.Users.Count();
    }
}
