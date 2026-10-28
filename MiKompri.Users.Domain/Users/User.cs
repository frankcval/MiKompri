using MiKompri.Users.Domain.Abstractions;

namespace MiKompri.Users.Domain.Users
{
    public class User : Entity
    {
        public string DisplayName { get; private set; } = string.Empty;
        public string? Email { get; private set; }

        // Enlace con el IdP OAuth/OIDC
        public string IdentityProvider { get; private set; } = string.Empty;
        public string? TenantId { get; private set; }
        public string? ObjectId { get; private set; }
        public string? ExternalUserId { get; private set; } // legado: sub

        private readonly List<GroupMembership> _memberships = new();
        public IReadOnlyCollection<GroupMembership> Memberships => _memberships.AsReadOnly();

        private User() { }

        public User(
            string displayName,
            string? email,
            string identityProvider,
            string? tenantId,
            string? objectId,
            string? externalUserId)
        {
            Id = Guid.NewGuid();
            DisplayName = displayName;
            Email = email;
            IdentityProvider = identityProvider;
            TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
            ObjectId = string.IsNullOrWhiteSpace(objectId) ? null : objectId;
            ExternalUserId = string.IsNullOrWhiteSpace(externalUserId) ? null : externalUserId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public User(string displayName, string? email, string identityProvider, string externalUserId)
            : this(displayName, email, identityProvider, null, null, externalUserId)
        {
        }

        public void AssignCanonicalIdentity(string tenantId, string objectId)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(objectId))
                throw new InvalidOperationException("La identidad canónica requiere tid y oid.");

            if (TenantId == tenantId && ObjectId == objectId)
                return;

            TenantId = tenantId;
            ObjectId = objectId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SyncClaims(string? displayName, string? email)
        {
            bool changed = false;

            if (displayName != null && displayName != DisplayName)
            {
                DisplayName = displayName;
                changed = true;
            }

            if (email != Email)
            {
                Email = email;
                changed = true;
            }

            if (changed)
                UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProfile(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new InvalidOperationException("El nombre no puede estar vacío.");

            if (displayName == DisplayName)
                return;

            DisplayName = displayName;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
