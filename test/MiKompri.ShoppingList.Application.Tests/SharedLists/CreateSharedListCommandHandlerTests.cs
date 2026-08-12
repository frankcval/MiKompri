using Moq;
using MiKompri.ShoppingList.Application.Commands.SharedLists.CreateSharedList;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class CreateSharedListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldThrow401_WhenUserIsNotAuthenticated()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var uow = new Mock<IUnitOfWork>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);
            currentUser.SetupGet(x => x.UserId).Returns(Guid.Empty);

            var handler = new CreateSharedListCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new CreateSharedListCommand("Lista", Guid.NewGuid(), null), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrow403_WhenMembershipIsInvalid()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var uow = new Mock<IUnitOfWork>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(false, false, null));

            var handler = new CreateSharedListCommandHandler(repo.Object, uow.Object, currentUser.Object, authz.Object);

            await Assert.ThrowsAsync<MiKompri.ShoppingList.Application.Exceptions.ForbiddenOperationException>(() =>
                handler.Handle(new CreateSharedListCommand("Lista", groupId, null), CancellationToken.None));
        }
    }
}
