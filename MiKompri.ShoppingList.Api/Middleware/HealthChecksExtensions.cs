using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace MiKompri.ShoppingList.Api.Middleware
{
    public static class HealthChecksExtensions
    {
        // Registrar health checks en el contenedor
        public static IServiceCollection AddMiKompriHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddHealthChecks()
                .AddNpgSql(
                    connectionString: configuration.GetConnectionString("PostgreSQL")!,
                    name: "postgresql",
                    tags: new[] { "db", "postgres" }
                );

            return services;
        }

        // Mapear endpoints de health en la app
        public static IEndpointRouteBuilder MapMiKompriHealthChecks(
            this IEndpointRouteBuilder endpoints)
        {
            // Health simple
            endpoints.MapHealthChecks("/health");

            // Health detallado
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
}


