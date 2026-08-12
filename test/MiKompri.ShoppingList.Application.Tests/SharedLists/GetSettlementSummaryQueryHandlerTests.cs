using Moq;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementSummary;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class GetSettlementSummaryQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnBalances_WhenListHasExpenses()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            var list = new PurchaseList("Shared", userA, groupId);
            var item = new ListItem(Guid.NewGuid(), "Leche", 1m, 1, userA);
            list.AddItem(item);
            item.RegisterExpense(userA, null, 10m, new[] { userA, userB });

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userA);
            repo.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);
            authz.Setup(x => x.GetMembershipAsync(groupId, userA, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Owner));

            var handler = new GetSettlementSummaryQueryHandler(repo.Object, currentUser.Object, authz.Object);
            var result = await handler.Handle(new GetSettlementSummaryQuery(list.Id), CancellationToken.None);

            Assert.Equal(2, result.Balances.Count);
            Assert.Contains(result.Balances, b => b.UserId == userA && b.NetBalance == 5m && b.Type == "Creditor");
            Assert.Contains(result.Balances, b => b.UserId == userB && b.NetBalance == -5m && b.Type == "Debtor");
        }
    }
}
