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

            if (request.ExpectedVersion is not null && user.ProfileVersion != request.ExpectedVersion)
                throw new ProfileConflictException();
            if (request.Details is { } details)
            {
                if (request.ExpectedVersion is null) throw new ProfileConflictException();
                var fields = request.Fields ?? new HashSet<string>();
                var patch = new ProfileDetails(request.DisplayName,
                    fields.Contains("username") ? details.Username : user.Username,
                    fields.Contains("givenName") ? details.GivenName : user.GivenName,
                    fields.Contains("familyName") ? details.FamilyName : user.FamilyName,
                    fields.Contains("preferredLanguage") ? details.PreferredLanguage : user.PreferredLanguage,
                    fields.Contains("defaultListSort") ? details.DefaultListSort : user.DefaultListSort);
                if (patch.Username is not null)
                {
                    var username = patch.Username.Trim().ToLowerInvariant();
                    var existing = await _userRepository.GetByUsernameAsync(username, cancellationToken);
                    if (existing is not null && existing.Id != user.Id) throw new UserConflictException();
                }
                user.UpdateDetails(patch, request.ExpectedVersion.Value);
            }
            else user.UpdateProfile(request.DisplayName);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return ToDto(user);
        }

        internal static UserProfileDto ToDto(User user) => new()
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            IdentityProvider = user.IdentityProvider,
            ExternalUserId = user.ExternalUserId ?? string.Empty,
            Username = user.Username,
            GivenName = user.GivenName,
            FamilyName = user.FamilyName,
            PreferredLanguage = user.PreferredLanguage,
            DefaultListSort = user.DefaultListSort,
            ProfileVersion = user.ProfileVersion,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
