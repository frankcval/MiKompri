using Moq;
using MiKompri.ShoppingList.Application.Commands.SharedLists.AddSharedItem;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Services;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class AddSharedItemCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldThrow403_WhenUserIsNotMember()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var uow = new Mock<IUnitOfWork>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var list = new PurchaseList("Shared", Guid.NewGuid(), groupId);

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            repo.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(false, false, null));

            var policy = new ActiveMembershipPolicy(authz.Object);
            var handler = new AddSharedItemCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object, policy);

            await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
                handler.Handle(new AddSharedItemCommand(list.Id, Guid.NewGuid(), "Leche", 1.5m, 1), CancellationToken.None));
        }
    }
}
