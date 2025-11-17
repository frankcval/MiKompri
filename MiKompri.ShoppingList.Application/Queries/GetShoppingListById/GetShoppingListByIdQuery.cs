using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.GetShoppingListById
{
    public record GetShoppingListByIdQuery(Guid Id) : IRequest<PurchaseListDTO>
    {
    }
}
