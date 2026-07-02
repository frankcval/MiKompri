using MediatR;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.Users.Application.Commands.UpdateProfile
{
    public sealed record UpdateProfileCommand(string DisplayName) : IRequest<UserProfileDto>;
}
