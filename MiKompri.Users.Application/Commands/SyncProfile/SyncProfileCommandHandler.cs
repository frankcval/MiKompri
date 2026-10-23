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
            var user = await _userRepository.GetByCanonicalIdentityAsync(
                request.IdentityProvider,
                request.TenantId,
                request.ObjectId,
                cancellationToken);

            if (user is null && !string.IsNullOrWhiteSpace(request.ExternalUserId))
            {
                user = await _userRepository.GetByExternalIdAsync(
                    request.IdentityProvider,
                    request.ExternalUserId,
                    cancellationToken);
            }

            if (user is null)
            {
                var email = string.IsNullOrEmpty(request.Email) ? null : request.Email;

                user = new User(
                    request.DisplayName ?? string.Empty,
                    email,
                    request.IdentityProvider,
                    request.TenantId,
                    request.ObjectId,
                    request.ExternalUserId);

                try
                {
                    await _userRepository.AddAsync(user, cancellationToken);
                    return (user.Id, Created: true);
                }
                catch
                {
                    var existingByCanonical = await _userRepository.GetByCanonicalIdentityAsync(
                        request.IdentityProvider,
                        request.TenantId,
                        request.ObjectId,
                        cancellationToken);

                    if (existingByCanonical is not null)
                    {
                        return (existingByCanonical.Id, Created: false);
                    }

                    if (!string.IsNullOrWhiteSpace(request.ExternalUserId))
                    {
                        var existingByExternalId = await _userRepository.GetByExternalIdAsync(
                            request.IdentityProvider,
                            request.ExternalUserId,
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
            user.AssignCanonicalIdentity(request.TenantId, request.ObjectId);
            user.SyncClaims(request.DisplayName, request.Email);

            if (user.UpdatedAt != previousUpdatedAt)
            {
                try
                {
                    await _userRepository.UpdateAsync(user, cancellationToken);
                }
                catch
                {
                    var existingByCanonical = await _userRepository.GetByCanonicalIdentityAsync(
                        request.IdentityProvider,
                        request.TenantId,
                        request.ObjectId,
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
    }
}
