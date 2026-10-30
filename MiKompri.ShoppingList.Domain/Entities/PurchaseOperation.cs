namespace MiKompri.ShoppingList.Domain.Entities;

// A durable receipt survives corrections/deletion of the financial record.
public sealed class PurchaseOperation
{
    public Guid Id { get; private set; }
    public Guid ActorId { get; private set; }
    public Guid ListId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid ExpenseId { get; private set; }
    public string RequestHash { get; private set; } = string.Empty;
    private PurchaseOperation() { }
    public PurchaseOperation(Guid id, Guid actorId, Guid listId, Guid itemId, Guid expenseId, string requestHash)
    {
        Id = id; ActorId = actorId; ListId = listId; ItemId = itemId; ExpenseId = expenseId; RequestHash = requestHash;
    }
}
