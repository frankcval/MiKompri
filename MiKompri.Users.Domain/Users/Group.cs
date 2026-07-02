using MiKompri.Users.Domain.Abstractions;

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
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            // El owner también es miembro con rol Owner, se inserta automáticamente
            var ownerMembership = GroupMembership.Create(Id, ownerId, GroupRole.Owner);
            _memberships.Add(ownerMembership);
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("El nombre del grupo no puede estar vacío.", nameof(newName));

            Name = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Devuelve el rol del miembro, o <c>null</c> si el usuario no pertenece al grupo.
        /// </summary>
        public GroupRole? GetMemberRole(Guid userId)
            => _memberships.FirstOrDefault(m => m.UserId == userId)?.Role;

        public GroupMembership AddMember(Guid userId, GroupRole role)
        {
            if (_memberships.Any(m => m.UserId == userId))
                throw new InvalidOperationException("El usuario ya pertenece al grupo.");

            var membership = GroupMembership.Create(Id, userId, role);
            _memberships.Add(membership);
            UpdatedAt = DateTime.UtcNow;

            return membership;
        }

        /// <summary>
        /// Elimina la membresía de <paramref name="targetUserId"/> según la matriz de privilegios.
        /// Orden de reglas (C1 → C2 → eliminación):
        /// <list type="number">
        ///   <item>Target no existe → <see cref="KeyNotFoundException"/> (400 vía middleware)</item>
        ///   <item>Último Owner → <see cref="InvalidOperationException"/> (400, FR-010)</item>
        ///   <item>Admin intenta eliminar no-Member → <see cref="ForbiddenOperationException"/> (403, FR-009)</item>
        /// </list>
        /// </summary>
        public void RemoveMember(Guid targetUserId, GroupRole requestingRole)
        {
            // (a) El target debe ser miembro
            var target = _memberships.FirstOrDefault(m => m.UserId == targetUserId);
            if (target is null)
                throw new KeyNotFoundException("El usuario no es miembro del grupo.");

            // (b) Protección del último Owner [C1, FR-010]
            if (target.Role == GroupRole.Owner && HasSingleOwner())
                throw new InvalidOperationException(
                    "No se puede eliminar al último propietario del grupo.");

            // (c) Un Admin solo puede eliminar Members [C2, FR-009]
            if (requestingRole == GroupRole.Admin && target.Role != GroupRole.Member)
                throw new ForbiddenOperationException(
                    "Un Admin solo puede eliminar miembros con rol Member.");

            _memberships.Remove(target);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeMemberRole(Guid userId, GroupRole newRole)
        {
            var membership = _memberships.FirstOrDefault(m => m.UserId == userId);
            if (membership is null)
                throw new KeyNotFoundException("El usuario no es miembro del grupo.");

            membership.ChangeRole(newRole);
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsOwner(Guid userId) => OwnerId == userId;

        // --- helpers privados ---

        private bool HasSingleOwner()
            => _memberships.Count(m => m.Role == GroupRole.Owner) == 1;
    }
}
