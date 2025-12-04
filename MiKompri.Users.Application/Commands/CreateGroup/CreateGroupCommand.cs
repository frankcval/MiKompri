using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Commands.CreateGroup
{
    public record CreateGroupCommand(string Name) : IRequest<Guid>;
}
