using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Infrastructure.Persistence;
using System.Linq;

namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest
{
    public class CustomWebApplicationFactory<Program>
     : WebApplicationFactory<Program> where Program : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Buscar el registro existente del DbContext (Npgsql)
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ShoppingListDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // 2. Crear un service provider aislado para el proveedor InMemory
                var inMemoryServiceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // 2. Registrar un DbContext EN MEMORIA para los tests
                services.AddDbContext<ShoppingListDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ShoppingListTestDb");
                    options.UseInternalServiceProvider(inMemoryServiceProvider);

                });

                builder.UseEnvironment("Development");

                // Opcional: puedes inicializar datos aquí si quieres
            });
        }
    }
}
