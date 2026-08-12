using Moq;
using MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateSharedList;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class UpdateSharedListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldThrow403_WhenRoleIsMember()
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
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Member));

            var handler = new UpdateSharedListCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object);

            await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
                handler.Handle(new UpdateSharedListCommand(list.Id, "Nuevo", "Desc"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldUpdateNameAndDescription_WhenRoleIsOwner()
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
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Owner));

            var handler = new UpdateSharedListCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object);

            await handler.Handle(new UpdateSharedListCommand(list.Id, "Nuevo", "Desc"), CancellationToken.None);

            Assert.Equal("Nuevo", list.Name);
            repo.Verify(x => x.UpdateAsync(list), Times.Once);
            uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
