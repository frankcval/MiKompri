using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Dtos;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Application.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IUserRepository _userRepository;

        public UpdateProfileCommandHandler(
            ICurrentUserService currentUser,
            IUserRepository userRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
        }

        public async Task<UserProfileDto> Handle(
            UpdateProfileCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.UserId, cancellationToken)
                ?? throw new KeyNotFoundException("Perfil de usuario no encontrado.");

            user.UpdateProfile(request.DisplayName);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return ToDto(user);
        }

        internal static UserProfileDto ToDto(User user) => new()
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            IdentityProvider = user.IdentityProvider,
            ExternalUserId = user.ExternalUserId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
