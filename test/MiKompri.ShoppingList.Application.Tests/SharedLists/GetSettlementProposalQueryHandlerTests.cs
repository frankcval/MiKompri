using Moq;
using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementProposal;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementSummary;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class GetSettlementProposalQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnDeterministicTransfers()
        {
            var sender = new Mock<ISender>();
            var listId = Guid.NewGuid();
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            var summary = new SettlementSummaryDto
            {
                SharedListId = listId,
                Balances = new List<ParticipantBalanceDto>
                {
                    new() { UserId = userA, NetBalance = 4.30m, TotalPaid = 8.30m, TotalOwed = 4m, Type = "Creditor" },
                    new() { UserId = userB, NetBalance = -4.30m, TotalPaid = 2m, TotalOwed = 6.30m, Type = "Debtor" }
                }
            };

            sender.Setup(x => x.Send(It.IsAny<GetSettlementSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);

            var handler = new GetSettlementProposalQueryHandler(sender.Object);

            var first = await handler.Handle(new GetSettlementProposalQuery(listId), CancellationToken.None);
            var second = await handler.Handle(new GetSettlementProposalQuery(listId), CancellationToken.None);

            Assert.Single(first.Transfers);
            Assert.Equal(first.Transfers[0].FromUserId, second.Transfers[0].FromUserId);
            Assert.Equal(first.Transfers[0].ToUserId, second.Transfers[0].ToUserId);
            Assert.Equal(first.Transfers[0].Amount, second.Transfers[0].Amount);
        }
    }
}
