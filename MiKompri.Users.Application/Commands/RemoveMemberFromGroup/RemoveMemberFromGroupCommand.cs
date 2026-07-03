using MediatR;

namespace MiKompri.Users.Application.Commands.RemoveMemberFromGroup
{
    public sealed record RemoveMemberFromGroupCommand(
        Guid GroupId,
        Guid TargetUserId
    ) : IRequest;
}
