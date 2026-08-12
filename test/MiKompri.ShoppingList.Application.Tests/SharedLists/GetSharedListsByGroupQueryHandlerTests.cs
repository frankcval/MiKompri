using Moq;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListsByGroup;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class GetSharedListsByGroupQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldThrow403_WhenMembershipIsInvalid()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(false, false, null));

            var handler = new GetSharedListsByGroupQueryHandler(repo.Object, currentUser.Object, authz.Object);

            await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
                handler.Handle(new GetSharedListsByGroupQuery(groupId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnGroupLists_WhenMembershipIsValid()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var lists = new[]
            {
                new PurchaseList("Lista A", Guid.NewGuid(), groupId),
                new PurchaseList("Lista B", Guid.NewGuid(), groupId)
            };

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Member));
            repo.Setup(x => x.GetByGroupAsync(groupId)).ReturnsAsync(lists);

            var handler = new GetSharedListsByGroupQueryHandler(repo.Object, currentUser.Object, authz.Object);
            var result = await handler.Handle(new GetSharedListsByGroupQuery(groupId), CancellationToken.None);

            Assert.Equal(2, result.Count());
        }
    }
}
