using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Services;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementSummary
{
    public sealed class GetSettlementSummaryQueryHandler : IRequestHandler<GetSettlementSummaryQuery, SettlementSummaryDto>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public GetSettlementSummaryQueryHandler(
            IPurchaseListRepository repository,
            ICurrentUserService currentUser,
            IGroupAuthorizationService groupAuthorization)
        {
            _repository = repository;
            _currentUser = currentUser;
            _groupAuthorization = groupAuthorization;
        }

        public async Task<SettlementSummaryDto> Handle(GetSettlementSummaryQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Authentication failed.");
            }

            var list = await _repository.GetByIdAsync(request.SharedListId)
                ?? throw new KeyNotFoundException("Lista no encontrada.");

            if (!list.IsShared || !list.GroupId.HasValue)
            {
                throw new ForbiddenOperationException("La operación solo aplica a listas compartidas.");
            }

            var membership = await _groupAuthorization.GetMembershipAsync(list.GroupId.Value, _currentUser.UserId, cancellationToken);
            if (!membership.IsAuthorizedMember)
            {
                throw new ForbiddenOperationException("No tienes acceso al settlement de esta lista compartida.");
            }

            var expenses = list.Items.SelectMany(x => x.Expenses).ToList();

            var payments = expenses.Select(x => (x.PaidBy, x.RealPaidPrice));
            var shares = expenses.SelectMany(x => x.Participants.Select(p => (p.ParticipantUserId, p.ShareAmount)));

            var netBalances = ExpenseSettlementCalculator.ComputeNetBalances(payments, shares);
            var totalPaidByUser = payments.GroupBy(x => x.PaidBy).ToDictionary(g => g.Key, g => g.Sum(x => x.RealPaidPrice));
            var totalOwedByUser = shares.GroupBy(x => x.ParticipantUserId).ToDictionary(g => g.Key, g => g.Sum(x => x.ShareAmount));

            var participants = totalPaidByUser.Keys
                .Union(totalOwedByUser.Keys)
                .Union(netBalances.Keys)
                .OrderBy(x => x)
                .ToList();

            var balances = participants.Select(userId =>
            {
                var totalPaid = totalPaidByUser.TryGetValue(userId, out var paid) ? paid : 0m;
                var totalOwed = totalOwedByUser.TryGetValue(userId, out var owed) ? owed : 0m;
                var net = netBalances.TryGetValue(userId, out var b) ? b : totalPaid - totalOwed;
                net = Math.Round(net, 2, MidpointRounding.AwayFromZero);

                var type = net > 0 ? "Creditor" : net < 0 ? "Debtor" : "Settled";

                return new ParticipantBalanceDto
                {
                    UserId = userId,
                    TotalPaid = Math.Round(totalPaid, 2, MidpointRounding.AwayFromZero),
                    TotalOwed = Math.Round(totalOwed, 2, MidpointRounding.AwayFromZero),
                    NetBalance = net,
                    Type = type
                };
            }).ToList();

            return new SettlementSummaryDto
            {
                SharedListId = list.Id,
                Balances = balances
            };
        }
    }
}
