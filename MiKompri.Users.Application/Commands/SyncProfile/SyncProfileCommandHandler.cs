using MediatR;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Application.Commands.SyncProfile
{
    public class SyncProfileCommandHandler
        : IRequestHandler<SyncProfileCommand, (Guid UserId, bool Created)>
    {
        private readonly IUserRepository _userRepository;

        public SyncProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(Guid UserId, bool Created)> Handle(
            SyncProfileCommand request,
            CancellationToken cancellationToken)
        {
            var identityProvider = LimitRequired(request.IdentityProvider, 100);
            var tenantId = LimitOptional(request.TenantId, 100);
            var objectId = LimitOptional(request.ObjectId, 100);
            var externalUserId = LimitOptional(request.ExternalUserId, 200);
            var email = LimitOptional(request.Email, 200);
            var displayName = BuildDisplayName(request.DisplayName, email);

            var user = await _userRepository.GetByCanonicalIdentityAsync(
                identityProvider,
                tenantId!,
                objectId!,
                cancellationToken);

            if (user is null && !string.IsNullOrWhiteSpace(externalUserId))
            {
                user = await _userRepository.GetByExternalIdAsync(
                    identityProvider,
                    externalUserId,
                    cancellationToken);
            }

            if (user is null)
            {
                user = new User(
                    displayName,
                    email,
                    identityProvider,
                    tenantId,
                    objectId,
                    externalUserId);

                try
                {
                    await _userRepository.AddAsync(user, cancellationToken);
                    return (user.Id, Created: true);
                }
                catch
                {
                    var existingByCanonical = await _userRepository.GetByCanonicalIdentityAsync(
                        identityProvider,
                        tenantId!,
                        objectId!,
                        cancellationToken);

                    if (existingByCanonical is not null)
                    {
                        return (existingByCanonical.Id, Created: false);
                    }

                    if (!string.IsNullOrWhiteSpace(externalUserId))
                    {
                        var existingByExternalId = await _userRepository.GetByExternalIdAsync(
                            identityProvider,
                            externalUserId,
                            cancellationToken);

                        if (existingByExternalId is not null)
                        {
                            return (existingByExternalId.Id, Created: false);
                        }
                    }

                    throw;
                }
            }

            var previousUpdatedAt = user.UpdatedAt;
            user.AssignCanonicalIdentity(tenantId!, objectId!);
            user.SyncClaims(displayName, email);

            if (user.UpdatedAt != previousUpdatedAt)
            {
                try
                {
                    await _userRepository.UpdateAsync(user, cancellationToken);
                }
                catch
                {
                    var existingByCanonical = await _userRepository.GetByCanonicalIdentityAsync(
                        identityProvider,
                        tenantId!,
                        objectId!,
                        cancellationToken);

                    if (existingByCanonical is not null)
                    {
                        return (existingByCanonical.Id, Created: false);
                    }

                    throw;
                }
            }

            return (user.Id, Created: false);
        }

        private static string BuildDisplayName(string? displayName, string? email)
        {
            var candidate = string.IsNullOrWhiteSpace(displayName) ? email : displayName;
            if (string.IsNullOrWhiteSpace(candidate))
            {
                candidate = "Usuario";
            }

            return LimitRequired(candidate, 100);
        }

        private static string? LimitOptional(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }

        private static string LimitRequired(string value, int maxLength)
        {
            var normalized = LimitOptional(value, maxLength);
            if (normalized is null)
            {
                throw new InvalidOperationException("El valor requerido de identidad no puede estar vacío.");
            }

            return normalized;
        }
    }
}
