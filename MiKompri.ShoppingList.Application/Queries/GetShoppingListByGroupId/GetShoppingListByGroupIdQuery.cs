using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.GetShoppingListByGroupId
{
    public record GetShoppingListByGroupIdQuery(Guid GroupId) : IRequest<IEnumerable<PurchaseListDTO>>
    {
    }
}
