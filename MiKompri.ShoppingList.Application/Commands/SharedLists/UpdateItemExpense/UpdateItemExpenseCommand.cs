using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateItemExpense
{
    public sealed record UpdateItemExpenseCommand(
        Guid SharedListId,
        Guid ItemId,
        Guid ExpenseId,
        Guid PaidBy,
        Guid? PurchasedBy,
        decimal RealPaidPrice,
        string? Currency,
        IReadOnlyCollection<Guid> Participants) : IRequest;
}
