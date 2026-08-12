using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest
{
    public sealed class TestGroupAuthorizationService : IGroupAuthorizationService
    {
        public static readonly Guid AllowedUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public Task<GroupAuthorizationResult> GetMembershipAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        {
            if (userId == AllowedUserId)
            {
                return Task.FromResult(new GroupAuthorizationResult(true, true, GroupRole.Owner));
            }

            return Task.FromResult(new GroupAuthorizationResult(false, false, null));
        }
    }
}
