using Moq;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListById;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class GetSharedListByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldThrow403_WhenUserIsNotActiveMember()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var list = new PurchaseList("Lista", Guid.NewGuid(), groupId);

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            repo.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(false, false, null));

            var handler = new GetSharedListByIdQueryHandler(repo.Object, currentUser.Object, authz.Object);

            await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
                handler.Handle(new GetSharedListByIdQuery(list.Id), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnDto_WhenUserIsActiveMember()
        {
            var repo = new Mock<IPurchaseListRepository>();
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

            var handler = new GetSharedListByIdQueryHandler(repo.Object, currentUser.Object, authz.Object);
            var result = await handler.Handle(new GetSharedListByIdQuery(list.Id), CancellationToken.None);

            Assert.Equal(list.Id, result.Id);
        }
    }
}
