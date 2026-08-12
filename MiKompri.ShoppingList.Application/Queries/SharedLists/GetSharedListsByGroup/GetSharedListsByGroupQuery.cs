using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListsByGroup
{
    public sealed record GetSharedListsByGroupQuery(Guid GroupId) : IRequest<IEnumerable<PurchaseListDTO>>;
}
