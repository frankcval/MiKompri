using Moq;
using MiKompri.ShoppingList.Application.Commands.SharedLists.DeleteItemExpense;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class DeleteItemExpenseCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeleteExpense_WhenRoleIsOwner()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var uow = new Mock<IUnitOfWork>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var list = new PurchaseList("Shared", Guid.NewGuid(), groupId);
            var item = new ListItem(Guid.NewGuid(), "Leche", 1m, 1, userId);
            list.AddItem(item);
            var expense = item.RegisterExpense(userId, null, 10m, new[] { userId });

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            repo.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Owner));

            var handler = new DeleteItemExpenseCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object);
            await handler.Handle(new DeleteItemExpenseCommand(list.Id, item.Id, expense.Id), CancellationToken.None);

            Assert.Empty(item.Expenses);
        }
    }
}
