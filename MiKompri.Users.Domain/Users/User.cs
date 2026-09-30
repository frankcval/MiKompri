using MiKompri.Users.Domain.Abstractions;

namespace MiKompri.Users.Domain.Users
{
    public class User : Entity
    {
        public string DisplayName { get; private set; } = string.Empty;
        public string? Email { get; private set; }

        // Enlace con el IdP OAuth/OIDC
        public string IdentityProvider { get; private set; } = string.Empty; // "keycloak", "auth0", "entra", "mikompri-auth"
        // Identidad legacy: "sub" del token. Nullable: los usuarios nuevos se correlacionan por (TenantId, ObjectId).
        // Nunca se almacena cadena vacía (TP11, data-model.md).
        public string? ExternalUserId { get; private set; }

        // Identidad canónica (TP11): claims "tid" y "oid" de Microsoft Entra ID.
        public string? TenantId { get; private set; }
        public string? ObjectId { get; private set; }

        // Navegación a memberships (opcional para dominio, pero útil)
        private readonly List<GroupMembership> _memberships = new();
        public IReadOnlyCollection<GroupMembership> Memberships => _memberships.AsReadOnly();

        // Constructor privado para EF
        private User() { }

        public User(
            string displayName,
            string? email,
            string identityProvider,
            string externalUserId)
        {
            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentException("ExternalUserId no puede ser vacío.", nameof(externalUserId));

            Id = Guid.NewGuid();
            DisplayName = displayName;
            Email = email;
            IdentityProvider = identityProvider;
            ExternalUserId = externalUserId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Crea un usuario nuevo correlacionado únicamente por la identidad canónica (tid, oid).
        /// <see cref="ExternalUserId"/> queda en null.
        /// </summary>
        public static User CreateFromCanonicalIdentity(
            string displayName,
            string? email,
            string identityProvider,
            string tenantId,
            string objectId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentException("tid no puede ser vacío.", nameof(tenantId));
            if (string.IsNullOrWhiteSpace(objectId))
                throw new ArgumentException("oid no puede ser vacío.", nameof(objectId));

            return new User
            {
                Id = Guid.NewGuid(),
                DisplayName = displayName,
                Email = email,
                IdentityProvider = identityProvider,
                ExternalUserId = null,
                TenantId = tenantId,
                ObjectId = objectId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Asocia (tid, oid) a un perfil legacy sin cambiar su <see cref="Entity.Id"/>.
        /// Devuelve true si hubo un cambio.
        /// </summary>
        public bool AssociateCanonicalIdentity(string tenantId, string objectId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentException("tid no puede ser vacío.", nameof(tenantId));
            if (string.IsNullOrWhiteSpace(objectId))
                throw new ArgumentException("oid no puede ser vacío.", nameof(objectId));

            if (TenantId == tenantId && ObjectId == objectId)
                return false;

            TenantId = tenantId;
            ObjectId = objectId;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Sincroniza los claims del IdP con el perfil local.
        /// Solo actualiza <see cref="UpdatedAt"/> si hubo al menos un cambio real,
        /// garantizando idempotencia en llamadas consecutivas con los mismos datos.
        /// </summary>
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

        /// <summary>
        /// Actualiza el nombre visible por iniciativa del usuario (FR-004).
        /// El email no es editable directamente por el usuario; se sincroniza vía <see cref="SyncClaims"/>.
        /// </summary>
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
