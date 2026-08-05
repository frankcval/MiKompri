using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace MiKompri.ProductCatalog.Api.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddMiKompriHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProductCatalogPostgreSQL")
                               ?? configuration.GetConnectionString("PostgreSQL");

        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: connectionString!,
                name: "postgresql",
                tags: new[] { "db", "postgres" });

        return services;
    }

    public static IEndpointRouteBuilder MapMiKompriHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");

        endpoints.MapHealthChecks("/health/details", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description
                    })
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        });

        return endpoints;
    }
}
