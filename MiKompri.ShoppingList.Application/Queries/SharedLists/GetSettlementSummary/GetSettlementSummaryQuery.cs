using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementSummary
{
    public sealed record GetSettlementSummaryQuery(Guid SharedListId) : IRequest<SettlementSummaryDto>;
}
