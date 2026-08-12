using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.DeleteItemExpense
{
    public sealed record DeleteItemExpenseCommand(Guid SharedListId, Guid ItemId, Guid ExpenseId) : IRequest;
}
