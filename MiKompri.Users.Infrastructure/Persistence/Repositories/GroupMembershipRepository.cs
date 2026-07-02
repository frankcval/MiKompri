using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Infrastructure.Persistence.Repositories
{
    public class GroupMembershipRepository : IGroupMembershipRepository
    {
        private readonly UsersDbContext _context;

        public GroupMembershipRepository(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<GroupMembership>> GetByGroupIdAsync(
            Guid groupId,
            CancellationToken cancellationToken = default)
        {
            return await _context.GroupMemberships
                .Where(m => m.GroupId == groupId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<GroupMembership>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.GroupMemberships
                .Where(m => m.UserId == userId)
                .ToListAsync(cancellationToken);
        }
    }
}
