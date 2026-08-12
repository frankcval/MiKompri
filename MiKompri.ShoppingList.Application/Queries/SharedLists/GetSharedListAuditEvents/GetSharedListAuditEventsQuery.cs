using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListAuditEvents
{
    public sealed record GetSharedListAuditEventsQuery(Guid SharedListId) : IRequest<IEnumerable<SharedListAuditEventDto>>;
}
