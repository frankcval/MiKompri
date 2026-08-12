using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Services
{
    public sealed class ActiveMembershipPolicy
    {
        private readonly IGroupAuthorizationService _groupAuthorization;

        public ActiveMembershipPolicy(IGroupAuthorizationService groupAuthorization)
        {
            _groupAuthorization = groupAuthorization;
        }

        public async Task EnsureActiveMemberAsync(Guid groupId, Guid userId, string roleInOperation, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                throw new ForbiddenOperationException($"El usuario para '{roleInOperation}' es inválido.");
            }

            var membership = await _groupAuthorization.GetMembershipAsync(groupId, userId, cancellationToken);
            if (!membership.IsAuthorizedMember)
            {
                throw new ForbiddenOperationException($"El usuario '{userId}' no tiene membresía activa para '{roleInOperation}'.");
            }
        }

        public async Task EnsureActiveMembersAsync(Guid groupId, IEnumerable<Guid> userIds, string roleInOperation, CancellationToken cancellationToken)
        {
            var uniqueUsers = userIds.Distinct().ToList();
            foreach (var userId in uniqueUsers)
            {
                await EnsureActiveMemberAsync(groupId, userId, roleInOperation, cancellationToken);
            }
        }
    }
}
