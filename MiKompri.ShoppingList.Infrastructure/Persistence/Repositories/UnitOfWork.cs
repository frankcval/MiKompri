using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork(ShoppingListDbContext context) : IUnitOfWork
    {
        private readonly ShoppingListDbContext _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
