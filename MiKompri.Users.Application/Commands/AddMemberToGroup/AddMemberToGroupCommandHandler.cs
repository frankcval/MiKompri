using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Commands.AddMemberToGroup
{
    public class AddMemberToGroupCommandHandler : IRequestHandler<AddMemberToGroupCommand>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;

        public AddMemberToGroupCommandHandler(
            ICurrentUserService currentUser,
            IGroupRepository groupRepository,
            IUserRepository userRepository)
        {
            _currentUser = currentUser;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
        }


        public async Task Handle(AddMemberToGroupCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                throw new InvalidOperationException("Usuario no autenticado.");

            var currentUserId = _currentUser.UserId;

            // 1) Cargar el grupo
            var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken)
                        ?? throw new KeyNotFoundException("Grupo no encontrado.");

            // 2) Regla de negocio: solo el owner puede añadir miembros
            if (!group.IsOwner(currentUserId))
                throw new InvalidOperationException("Solo el propietario del grupo puede añadir miembros.");

            // 3) Cargar el usuario a añadir
            var memberUser = await _userRepository.GetByIdAsync(request.MemberUserId, cancellationToken)
                             ?? throw new KeyNotFoundException("Usuario a añadir no encontrado.");

            // 4) Añadir al miembro
            group.AddMember(memberUser.Id, request.Role);

            // 5) Guardar cambios
            await _groupRepository.UpdateAsync(group, cancellationToken);
        }
    }
}
