using Microsoft.EntityFrameworkCore;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence
{
    public class ShoppingListDbContext : DbContext
    {
        public DbSet<PurchaseList> PurchaseList => Set<PurchaseList>();
        public DbSet<ListItem> ListItems => Set<ListItem>();

        public ShoppingListDbContext(DbContextOptions<ShoppingListDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Busca en este ensamblado todas las clases que implementen IEntityTypeConfiguration<T> y aplícalas automáticamente
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShoppingListDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

    }

}
