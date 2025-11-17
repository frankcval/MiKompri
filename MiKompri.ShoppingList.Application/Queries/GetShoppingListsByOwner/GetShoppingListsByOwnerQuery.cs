using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.GetShoppingListsByOwner
{
    public record GetShoppingListsByOwnerQuery(Guid OwnerId) : IRequest<IEnumerable<PurchaseListDTO>>
    {
    }
}
