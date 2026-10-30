using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.ProductCatalog.Infrastructure.Persistence;

namespace MiKompri.ProductCatalog.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"ProductCatalogApiTests-{Guid.NewGuid()}";
    private string? _postgresAdmin;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Authentication:Authority"] = "https://issuer.local",
                ["Authentication:Audience"] = "api://mikompri-backend",
                ["Authentication:ValidAudiences"] = "api://mikompri-backend,mikompri-productcatalog-api",
                ["ConnectionStrings:PostgreSQL"] = "Host=localhost;Port=5432;Database=MiKompri_ProductCatalog;Username=postgres;Password=12345"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureTestServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ProductCatalogDbContext>))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            var postgres = Environment.GetEnvironmentVariable("MIKOMPRI_TEST_POSTGRES");
            if (!string.IsNullOrWhiteSpace(postgres))
            {
                _postgresAdmin = postgres;
                using var admin = new Npgsql.NpgsqlConnection(postgres); admin.Open();
                using var create = new Npgsql.NpgsqlCommand($"CREATE DATABASE \"{_dbName}\"", admin); create.ExecuteNonQuery();
                var connection = new Npgsql.NpgsqlConnectionStringBuilder(postgres) { Database = _dbName }.ConnectionString;
                services.AddDbContext<ProductCatalogDbContext>(options => options.UseNpgsql(connection));
            }
            else
            {
                var inMemoryServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
                services.AddDbContext<ProductCatalogDbContext>(options => { options.UseInMemoryDatabase(_dbName); options.UseInternalServiceProvider(inMemoryServiceProvider); });
            }

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && _postgresAdmin is not null)
        {
            using var admin = new Npgsql.NpgsqlConnection(_postgresAdmin); admin.Open();
            using var drop = new Npgsql.NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_dbName}\" WITH (FORCE)", admin); drop.ExecuteNonQuery();
            _postgresAdmin = null;
        }
    }
}
