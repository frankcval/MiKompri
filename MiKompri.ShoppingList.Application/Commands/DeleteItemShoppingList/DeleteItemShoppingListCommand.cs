using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList
{
    public record class DeleteItemShoppingListCommand(Guid ListId, Guid ItemId) : IRequest<bool>
    {
    }
}
