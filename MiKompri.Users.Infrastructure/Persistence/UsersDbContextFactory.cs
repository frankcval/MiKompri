using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiKompri.Users.Infrastructure.Persistence
{
    public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
    {
        public UsersDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<UsersDbContext>()
                .UseNpgsql("Host=localhost;Database=mikompri_users_design;Username=postgres;Password=design")
                .Options;
            return new UsersDbContext(options);
        }
    }
}
