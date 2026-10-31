using MiKompri.Users.Domain.Abstractions;

namespace MiKompri.Users.Domain.Users
{
    public class User : Entity
    {
        public string DisplayName { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? Username { get; private set; }
        public string? GivenName { get; private set; }
        public string? FamilyName { get; private set; }
        public string? PreferredLanguage { get; private set; }
        public string DefaultListSort { get; private set; } = "newest";
        public long ProfileVersion { get; private set; }
        public bool IsSuspended { get; private set; }
        public long SecurityVersion { get; private set; }
        public long? TokensValidFromExclusive { get; private set; }

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

            if (TenantId is not null || ObjectId is not null)
                throw new InvalidOperationException("La identidad existente requiere una transición autorizada.");

            TenantId = tenantId;
            ObjectId = objectId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SyncClaims(string? displayName, string? email)
        {
            // Claims initialize only in the constructor. Existing rows are user-owned,
            // including intentional nulls; a login is not an instruction to reimport.
        }

        public void UpdateProfile(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new InvalidOperationException("El nombre no puede estar vacío.");

            if (displayName == DisplayName)
                return;

            DisplayName = displayName;
            ProfileVersion++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void InitializeNames(string? givenName, string? familyName)
        {
            // Called once by provisioning, never by resolve/login.
            if (ProfileVersion != 0 || GivenName is not null || FamilyName is not null)
                throw new InvalidOperationException("El perfil ya está inicializado.");
            GivenName = OptionalName(givenName);
            FamilyName = OptionalName(familyName);
        }

        public void UpdateDetails(ProfileDetails details, long expectedVersion)
        {
            if (ProfileVersion != expectedVersion) throw new ProfileConflictException();
            if (string.IsNullOrWhiteSpace(details.DisplayName) || details.DisplayName.Trim().Length > 100)
                throw new InvalidOperationException("El nombre debe tener entre 1 y 100 caracteres.");
            var username = string.IsNullOrWhiteSpace(details.Username) ? null : details.Username.Trim().ToLowerInvariant();
            if (username is not null && (!System.Text.RegularExpressions.Regex.IsMatch(username, "^[a-z][a-z0-9_]{2,29}$") ||
                new[] { "admin", "support", "system", "mikompri" }.Contains(username)))
                throw new InvalidOperationException("El username no es válido o está reservado.");
            if (details.PreferredLanguage is not null && details.PreferredLanguage != "es")
                throw new InvalidOperationException("Idioma no disponible.");
            if (!new[] { "newest", "oldest", "name", "updated" }.Contains(details.DefaultListSort))
                throw new InvalidOperationException("Orden de listas no válido.");
            var given = OptionalName(details.GivenName);
            var family = OptionalName(details.FamilyName);
            DisplayName = details.DisplayName.Trim();
            Username = username;
            GivenName = given;
            FamilyName = family;
            PreferredLanguage = details.PreferredLanguage;
            DefaultListSort = details.DefaultListSort;
            ProfileVersion++;
            UpdatedAt = DateTime.UtcNow;
        }

        private static string? OptionalName(string? name)
        {
            if (name is null) return null;
            var trimmed = name.Trim();
            if (trimmed.Length > 100) throw new InvalidOperationException("El nombre no puede superar 100 caracteres.");
            return trimmed.Length == 0 ? null : trimmed;
        }

        public void SetSuspended(bool suspended, DateTimeOffset now)
        {
            if (IsSuspended == suspended) return;
            IsSuspended = suspended;
            CutTokens(now);
        }

        public bool AcceptsIssuedAt(long? issuedAt) => TokensValidFromExclusive is null ||
            (issuedAt is not null && issuedAt > TokensValidFromExclusive);

        public void MoveCanonicalIdentity(string provider, string tenantId, string objectId, DateTimeOffset now)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(objectId))
                throw new InvalidOperationException("La identidad requiere proveedor, tenant y objeto.");
            IdentityProvider = provider;
            TenantId = tenantId;
            ObjectId = objectId;
            // Retired sub must never bootstrap this row back into the source namespace.
            ExternalUserId = null;
            CutTokens(now);
        }

        private void CutTokens(DateTimeOffset now)
        {
            TokensValidFromExclusive = Math.Max(TokensValidFromExclusive ?? 0, now.ToUnixTimeSeconds());
            SecurityVersion++;
            UpdatedAt = now.UtcDateTime;
        }
    }
}
