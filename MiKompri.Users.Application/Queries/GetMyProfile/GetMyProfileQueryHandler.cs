using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Commands.UpdateProfile;
using MiKompri.Users.Application.Dtos;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Application.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileDto>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IUserRepository _userRepository;

        public GetMyProfileQueryHandler(
            ICurrentUserService currentUser,
            IUserRepository userRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
        }

        public async Task<UserProfileDto> Handle(
            GetMyProfileQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.UserId, cancellationToken)
                ?? throw new KeyNotFoundException("Perfil de usuario no encontrado.");

            return UpdateProfileCommandHandler.ToDto(user);
        }
    }
}
