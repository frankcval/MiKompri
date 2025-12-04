using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MiKompri.Users.Application.Behavior;




namespace MiKompri.Users.Application
{
    //Clase extension de depenencias, por convención se llama DependencyInjection o DependencyInjectionExtensions
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            // FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Registro del pipeline behavior de logging
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Pipeline de validación
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
