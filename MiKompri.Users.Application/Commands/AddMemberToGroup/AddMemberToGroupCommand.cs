using MediatR;
using MiKompri.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Commands.AddMemberToGroup
{
    public sealed record AddMemberToGroupCommand(
    Guid GroupId,
    Guid MemberUserId,
    GroupRole Role
) : IRequest;
}
