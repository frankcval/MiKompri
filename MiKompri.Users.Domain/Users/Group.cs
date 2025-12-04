using MiKompri.Users.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Domain.Users
{
    public class Group : Entity
    {
        public string Name { get; private set; } = string.Empty;

        // Dueño del grupo (UserId interno)
        public Guid OwnerId { get; private set; }

        private readonly List<GroupMembership> _memberships = new();
        public IReadOnlyCollection<GroupMembership> Memberships => _memberships.AsReadOnly();

        private Group() { } // para EF

        public Group(string name, Guid ownerId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del grupo no puede estar vacío.", nameof(name));

            Id = Guid.NewGuid();
            Name = name;
            OwnerId = ownerId;

            // El owner también es miembro con rol Owner, se inserta automáticamente
            var ownerMembership = GroupMembership.Create(Id, ownerId, GroupRole.Owner);
            _memberships.Add(ownerMembership);
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("El nombre del grupo no puede estar vacío.", nameof(newName));

            Name = newName;
        }

        public GroupMembership AddMember(Guid userId, GroupRole role)
        {
            if (_memberships.Any(m => m.UserId == userId))
                throw new InvalidOperationException("El usuario ya pertenece al grupo.");

            var membership = GroupMembership.Create(Id, userId, role);
            _memberships.Add(membership);

            return membership;
        }

        public void RemoveMember(Guid userId)
        {
            var membership = _memberships.FirstOrDefault(m => m.UserId == userId);
            if (membership is null)
                throw new KeyNotFoundException("El usuario no es miembro del grupo.");

            if (membership.Role == GroupRole.Owner)
                throw new InvalidOperationException("No se puede eliminar al dueño del grupo.");

            _memberships.Remove(membership);
        }

        public void ChangeMemberRole(Guid userId, GroupRole newRole)
        {
            var membership = _memberships.FirstOrDefault(m => m.UserId == userId);
            if (membership is null)
                throw new KeyNotFoundException("El usuario no es miembro del grupo.");

            membership.ChangeRole(newRole);
        }

        public bool IsOwner(Guid userId) => OwnerId == userId;
    }
}
