using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.RegisterItemExpense
{
    public sealed record RegisterItemExpenseCommand(
        Guid SharedListId,
        Guid ItemId,
        Guid PaidBy,
        Guid? PurchasedBy,
        decimal RealPaidPrice,
        string? Currency,
        IReadOnlyCollection<Guid> Participants) : IRequest<Guid>;
}
