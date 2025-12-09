using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.Users.Domain.Users;
using MiKompri.Users.Infrastructure.Persistence;
using MiKompri.Users.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("UsersPostgreSQL")
                                   ?? configuration.GetConnectionString("PostgreSQL");

            services.AddDbContext<UsersDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });




            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IGroupMembershipRepository, GroupMembershipRepository>();

            return services;
        }
    }
}
