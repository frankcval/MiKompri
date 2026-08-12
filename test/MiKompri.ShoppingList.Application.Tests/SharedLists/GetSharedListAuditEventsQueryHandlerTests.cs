using Moq;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListAuditEvents;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class GetSharedListAuditEventsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnAuditEvents_WhenUserIsAuthorizedMember()
        {
            var repo = new Mock<IPurchaseListRepository>();
            var currentUser = new Mock<ICurrentUserService>();
            var authz = new Mock<IGroupAuthorizationService>();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var list = new PurchaseList("Shared", Guid.NewGuid(), groupId);
            var auditEvent = new SharedListAuditEvent(list.Id, userId, "ItemAdded", nameof(ListItem), Guid.NewGuid());

            currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
            currentUser.SetupGet(x => x.UserId).Returns(userId);
            repo.Setup(x => x.GetByIdAsync(list.Id)).ReturnsAsync(list);
            repo.Setup(x => x.GetSharedListAuditEventsAsync(list.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SharedListAuditEvent> { auditEvent });
            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Member));

            var handler = new GetSharedListAuditEventsQueryHandler(repo.Object, currentUser.Object, authz.Object);
            var result = (await handler.Handle(new GetSharedListAuditEventsQuery(list.Id), CancellationToken.None)).ToList();

            Assert.Single(result);
            Assert.Equal(auditEvent.Id, result[0].Id);
            Assert.Equal("ItemAdded", result[0].ActionType);
        }
    }
}
