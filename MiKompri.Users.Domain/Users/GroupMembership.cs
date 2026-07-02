using MiKompri.Users.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Domain.Users
{
    public class GroupMembership : Entity
    {
        public Guid GroupId { get; private set; }
        public Guid UserId { get; private set; }
        public GroupRole Role { get; private set; }

        private GroupMembership() { } // para EF

        private GroupMembership(Guid groupId, Guid userId, GroupRole role)
        {
            Id = Guid.NewGuid();
            GroupId = groupId;
            UserId = userId;
            Role = role;
        }

        public static GroupMembership Create(Guid groupId, Guid userId, GroupRole role)
            => new(groupId, userId, role);

        public void ChangeRole(GroupRole newRole)
        {
            // podrías meter reglas (no permitir bajar de Owner a Member si no hay otro Owner, etc.)
            Role = newRole;
        }
    }
}
