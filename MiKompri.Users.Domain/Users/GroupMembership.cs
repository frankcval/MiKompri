using MiKompri.Users.Domain.Abstractions;

namespace MiKompri.Users.Domain.Users
{
    public class GroupMembership : Entity
    {
        public Guid GroupId { get; private set; }
        public Guid UserId { get; private set; }
        public GroupRole Role { get; private set; }
        public DateTime JoinedAt { get; private set; }

        private GroupMembership() { } // para EF

        private GroupMembership(Guid groupId, Guid userId, GroupRole role)
        {
            Id = Guid.NewGuid();
            GroupId = groupId;
            UserId = userId;
            Role = role;
            JoinedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public static GroupMembership Create(Guid groupId, Guid userId, GroupRole role)
            => new(groupId, userId, role);

        public void ChangeRole(GroupRole newRole)
        {
            Role = newRole;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
