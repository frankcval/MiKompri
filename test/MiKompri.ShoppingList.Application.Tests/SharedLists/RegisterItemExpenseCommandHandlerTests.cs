using Moq;
using MiKompri.ShoppingList.Application.Commands.SharedLists.RegisterItemExpense;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Services;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class RegisterItemExpenseCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldThrow403_WhenMemberRegistersPaymentForAnotherUser()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var uow = new Mock<IUnitOfWork>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var list = new PurchaseList("Shared", Guid.NewGuid(), groupId);
            list.AddItem(new ListItem(Guid.NewGuid(), "Leche", 1m, 1, userId));
            var itemId = list.Items.First().Id;

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            repo.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Member));

            var policy = new ActiveMembershipPolicy(authz.Object);
            var handler = new RegisterItemExpenseCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object, policy);

            await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
                handler.Handle(new RegisterItemExpenseCommand(list.Id, itemId, Guid.NewGuid(), null, 10m, "EUR", new[] { userId }), CancellationToken.None));
        }
    }
}
