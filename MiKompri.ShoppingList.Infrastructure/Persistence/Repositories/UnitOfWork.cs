using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork(ShoppingListDbContext context) : IUnitOfWork
    {
        private readonly ShoppingListDbContext _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try { return await _context.SaveChangesAsync(cancellationToken); }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new MiKompri.ShoppingList.Application.Exceptions.ResourceConflictException("El gasto cambió mientras lo editabas. Actualiza los datos antes de corregirlo.");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505", TableName: "purchase_operations" })
            {
                throw new MiKompri.ShoppingList.Application.Exceptions.ResourceConflictException("La operación se está guardando. Reintenta con la misma operación para consultar su resultado.");
            }
        }
    }
}
