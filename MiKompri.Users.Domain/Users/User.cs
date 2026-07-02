using MiKompri.Users.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public void UpdateProfile(string displayName, string? email)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new InvalidOperationException("El nombre no puede estar vacío.");

            DisplayName = displayName;
            Email = email;
        }
    }
}
