using Moq;
using MiKompri.ShoppingList.Application.Commands.SharedLists.CloseSharedList;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class CloseSharedListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCloseList_WhenRoleIsAdmin()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var uow = new Mock<IUnitOfWork>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var list = new PurchaseList("Lista", Guid.NewGuid(), groupId);

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            repo.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Admin));

            var handler = new CloseSharedListCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object);

            await handler.Handle(new CloseSharedListCommand(list.Id), CancellationToken.None);

            Assert.Equal(SharedListStatus.Closed, list.Status);
            uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
