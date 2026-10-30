using Microsoft.EntityFrameworkCore;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;
namespace MiKompri.ShoppingList.Infrastructure.Persistence.Repositories;

public sealed class PurchaseOperationStore(ShoppingListDbContext context) : IPurchaseOperationStore
{
    public Task<PurchaseOperation?> FindAsync(Guid id, CancellationToken ct) => context.Set<PurchaseOperation>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> ExpenseExistsAsync(Guid id, CancellationToken ct) => context.ItemExpenseRecords.AnyAsync(x => x.Id == id, ct);
    public async Task AddAsync(PurchaseOperation operation, CancellationToken ct) => await context.Set<PurchaseOperation>().AddAsync(operation, ct);
}
