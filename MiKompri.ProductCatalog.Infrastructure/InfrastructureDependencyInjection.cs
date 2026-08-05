using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.ProductCatalog.Application.Abstractions;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Infrastructure.Persistence;
using MiKompri.ProductCatalog.Infrastructure.Persistence.Repositories;
using MiKompri.ProductCatalog.Infrastructure.Services;

namespace MiKompri.ProductCatalog.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddProductCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProductCatalogPostgreSQL")
                               ?? configuration.GetConnectionString("PostgreSQL");

        services.AddDbContext<ProductCatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICatalogProductRepository, CatalogProductRepository>();
        services.AddScoped<IMarketRepository, MarketRepository>();
        services.AddScoped<IProductPriceRecordRepository, ProductPriceRecordRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
