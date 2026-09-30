using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Infrastructure.Persistence;
using MiKompri.ShoppingList.Infrastructure.Persistence.Repositories;
using MiKompri.ShoppingList.Infrastructure.Services;

namespace MiKompri.ShoppingList.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ShoppingListDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

            services.AddScoped<IPurchaseListRepository, PurchaseListRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            var usersApiBaseUrl = configuration["UsersApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(usersApiBaseUrl))
                throw new InvalidOperationException("UsersApi:BaseUrl no puede estar vacío.");

            services.AddHttpClient("UsersApi", client =>
            {
                client.BaseAddress = new Uri(usersApiBaseUrl, UriKind.Absolute);
            });

            services.AddScoped<UsersGroupAuthorizationAdapter>();
            services.AddScoped<IGroupAuthorizationService>(sp => sp.GetRequiredService<UsersGroupAuthorizationAdapter>());
            services.AddScoped<IUserIdentityResolver>(sp => sp.GetRequiredService<UsersGroupAuthorizationAdapter>());

            return services;
        }
    }
}
