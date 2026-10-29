using MiKompri.ShoppingList.Domain.Entities;
namespace MiKompri.ShoppingList.Application.Interfaces;

public interface IPurchaseOperationStore
{
    Task<PurchaseOperation?> FindAsync(Guid id, CancellationToken ct);
    Task<bool> ExpenseExistsAsync(Guid id, CancellationToken ct);
    Task AddAsync(PurchaseOperation operation, CancellationToken ct);
}
