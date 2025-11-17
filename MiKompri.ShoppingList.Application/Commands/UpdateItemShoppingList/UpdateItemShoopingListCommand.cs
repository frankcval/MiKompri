using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList
{
    public record UpdateItemShoopingListCommand(Guid listId, Guid ProdId, string? ProductName, decimal? Price, int? Quantity) : IRequest<bool>
    {
    }
}
