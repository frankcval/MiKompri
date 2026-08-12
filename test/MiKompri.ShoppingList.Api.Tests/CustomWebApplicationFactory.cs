using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Infrastructure.Persistence;
using System.Linq;

namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest
{
    public class CustomWebApplicationFactory<Program>
     : WebApplicationFactory<Program> where Program : class
    {
        private readonly string _dbName = $"ShoppingListTestDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ShoppingListDbContext>))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                var inMemoryServiceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<ShoppingListDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                    options.UseInternalServiceProvider(inMemoryServiceProvider);
                });

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                var authzDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGroupAuthorizationService));
                if (authzDescriptor is not null)
                {
                    services.Remove(authzDescriptor);
                }
                services.AddSingleton<IGroupAuthorizationService, TestGroupAuthorizationService>();

                builder.UseEnvironment("Development");
            });
        }
    }
}
