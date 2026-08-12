using MediatR;
using MiKompri.ShoppingList.Application.DTOs;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementProposal
{
    public sealed record GetSettlementProposalQuery(Guid SharedListId) : IRequest<SettlementProposalDto>;
}
