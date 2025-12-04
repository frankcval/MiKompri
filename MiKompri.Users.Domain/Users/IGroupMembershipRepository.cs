using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Domain.Users
{
    public interface IGroupMembershipRepository
    {
        Task<IReadOnlyCollection<GroupMembership>> GetByGroupIdAsync(
            Guid groupId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<GroupMembership>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
