using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementSummary;
using MiKompri.ShoppingList.Domain.Services;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementProposal
{
    public sealed class GetSettlementProposalQueryHandler : IRequestHandler<GetSettlementProposalQuery, SettlementProposalDto>
    {
        private readonly ISender _sender;

        public GetSettlementProposalQueryHandler(ISender sender)
        {
            _sender = sender;
        }

        public async Task<SettlementProposalDto> Handle(GetSettlementProposalQuery request, CancellationToken cancellationToken)
        {
            var summary = await _sender.Send(new GetSettlementSummaryQuery(request.SharedListId), cancellationToken);

            var balances = summary.Balances.ToDictionary(x => x.UserId, x => x.NetBalance);
            var builder = new DeterministicSettlementProposalBuilder();
            var transfers = builder.Build(balances);

            if (!DeterministicSettlementProposalBuilder.IsWithinTheoreticalLimit(balances, transfers.Count))
            {
                throw new InvalidOperationException("La propuesta de liquidación excede el límite teórico D + A - 1.");
            }

            return new SettlementProposalDto
            {
                SharedListId = request.SharedListId,
                GeneratedAt = DateTime.UtcNow,
                Transfers = transfers
                    .Select(x => new SettlementTransferDto
                    {
                        FromUserId = x.FromUserId,
                        ToUserId = x.ToUserId,
                        Amount = x.Amount
                    })
                    .ToList()
            };
        }
    }
}
