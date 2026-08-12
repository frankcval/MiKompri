using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListById
{
    public sealed record GetSharedListByIdQuery(Guid SharedListId) : IRequest<PurchaseListDTO>;
}
