using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Infrastructure.Persistence.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly UsersDbContext _context;

        public GroupRepository(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Groups
                .Include(g => g.Memberships)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Group>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Groups
                .Include(g => g.Memberships)
                .Where(g => g.Memberships.Any(m => m.UserId == userId))
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Group group, CancellationToken cancellationToken = default)
        {
            await _context.Groups.AddAsync(group, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
