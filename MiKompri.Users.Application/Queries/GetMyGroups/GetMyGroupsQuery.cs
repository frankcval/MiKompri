using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Queries.GetMyGroups
{
    public sealed record GetMyGroupsQuery() : IRequest<IReadOnlyCollection<GroupDto>>;
    //No necesita parámetros:el usuario se deduce de ICurrentUserService.
}
