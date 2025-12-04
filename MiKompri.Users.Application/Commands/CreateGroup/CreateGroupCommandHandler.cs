using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Commands.CreateGroup
{
    public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Guid>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupRepository _groupRepository;

        public CreateGroupCommandHandler(
            ICurrentUserService currentUser,
            IGroupRepository groupRepository)
        {
            _currentUser = currentUser;
            _groupRepository = groupRepository;
        }

        public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                throw new InvalidOperationException("Usuario no autenticado.");

            var ownerId = _currentUser.UserId;

            // Usamos el dominio para crear el grupo
            var group = new Group(request.Name, ownerId);

            await _groupRepository.AddAsync(group, cancellationToken);

            return group.Id;
        }
    }
}
