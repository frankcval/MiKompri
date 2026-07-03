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
            var user = await _userRepository.GetByExternalIdAsync(
                request.IdentityProvider,
                request.ExternalUserId,
                cancellationToken);

            if (user is null)
            {
                // C4: email null si ausente — nunca cadena vacía
                var email = string.IsNullOrEmpty(request.Email) ? null : request.Email;

                user = new User(
                    request.DisplayName ?? string.Empty,
                    email,
                    request.IdentityProvider,
                    request.ExternalUserId);

                await _userRepository.AddAsync(user, cancellationToken);
                return (user.Id, Created: true);
            }

            // C5: guardar solo si SyncClaims produjo un cambio real (idempotencia)
            var previousUpdatedAt = user.UpdatedAt;
            var email = string.IsNullOrEmpty(request.Email) ? null : request.Email;
            user.SyncClaims(request.DisplayName, email);

            if (user.UpdatedAt != previousUpdatedAt)
                await _userRepository.UpdateAsync(user, cancellationToken);
            return (user.Id, Created: false);
        }
    }
}
