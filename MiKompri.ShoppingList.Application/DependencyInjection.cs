using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.ShoppingList.Application.Behavior;
using MiKompri.ShoppingList.Application.Services;
using System.Reflection;

namespace MiKompri.ShoppingList.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAplicaction(this IServiceCollection services)
        {

            var assembly = Assembly.GetExecutingAssembly();

            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(assembly));

            // FluentValidation: registra todos los validators de este assembly
            services.AddValidatorsFromAssembly(assembly);

            // Registro del pipeline behavior de logging
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Pipeline de validación
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<ActiveMembershipPolicy>();

            return services;
        }
    }
}
