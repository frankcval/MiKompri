using MediatR;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.Users.Application.Queries.GetGroupMembers
{
    public sealed record GetGroupMembersQuery(Guid GroupId)
        : IRequest<IReadOnlyCollection<GroupMemberDto>>;
}
