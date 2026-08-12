using Moq;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Services;

namespace MiKompri.ShoppingList.Application.Tests.SharedLists
{
    public class ActiveMembershipPolicyTests
    {
        [Fact]
        public async Task EnsureActiveMemberAsync_ShouldThrow_WhenMembershipIsInactive()
        {
            var authz = new Mock<IGroupAuthorizationService>();
            var policy = new ActiveMembershipPolicy(authz.Object);

            var groupId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            authz.Setup(x => x.GetMembershipAsync(groupId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, false, GroupRole.Member));

            await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
                policy.EnsureActiveMemberAsync(groupId, userId, "Participants", CancellationToken.None));
        }

        [Fact]
        public async Task EnsureActiveMembersAsync_ShouldPass_WhenAllMembersAreActive()
        {
            var authz = new Mock<IGroupAuthorizationService>();
            var policy = new ActiveMembershipPolicy(authz.Object);

            var groupId = Guid.NewGuid();
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            authz.Setup(x => x.GetMembershipAsync(groupId, userA, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Member));
            authz.Setup(x => x.GetMembershipAsync(groupId, userB, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupAuthorizationResult(true, true, GroupRole.Member));

            await policy.EnsureActiveMembersAsync(groupId, new[] { userA, userB }, "Participants", CancellationToken.None);
        }
    }
}
