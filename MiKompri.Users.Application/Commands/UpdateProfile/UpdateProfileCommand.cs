using MediatR;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.Users.Application.Commands.UpdateProfile
{
    public sealed record UpdateProfileCommand(string DisplayName,
        MiKompri.Users.Domain.Users.ProfileDetails? Details = null,
        IReadOnlySet<string>? Fields = null, long? ExpectedVersion = null) : IRequest<UserProfileDto>;
}
