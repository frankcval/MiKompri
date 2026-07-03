using MiKompri.Users.Domain.Abstractions;

namespace MiKompri.Users.Domain.Users
{
    public class User : Entity
    {
        public string DisplayName { get; private set; } = string.Empty;
        public string? Email { get; private set; }

        // Enlace con el IdP OAuth/OIDC
        public string IdentityProvider { get; private set; } = string.Empty; // "keycloak", "auth0", "entra", "mikompri-auth"
        public string ExternalUserId { get; private set; } = string.Empty;   // normalmente el "sub" del token

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
            Id = Guid.NewGuid();
            DisplayName = displayName;
            Email = email;
            IdentityProvider = identityProvider;
            ExternalUserId = externalUserId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
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
