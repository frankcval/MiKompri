using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.ProductCatalog.Application.Behavior;
using MiKompri.ProductCatalog.Application.Options;

namespace MiKompri.ProductCatalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProductCatalogApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddOptions<ProductCatalogOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
