using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Domain.Abstractions;
using MiKompri.Users.Domain.Users;

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
            var currentUserId = _currentUser.UserId;

            // (a) Grupo no encontrado → 403 para evitar enumeración de grupos [G1, SR-006]
            var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken)
                ?? throw new ForbiddenOperationException("Acceso denegado.");

            // (b) El caller debe ser Owner o Admin [C2, FR-012]
            var callerRole = group.GetMemberRole(currentUserId);
            if (callerRole is null || callerRole == GroupRole.Member)
                throw new ForbiddenOperationException("No tienes permisos para añadir miembros a este grupo.");

            // (c) Un Admin no puede asignar rol Admin [C2, FR-007]
            if (callerRole == GroupRole.Admin && request.Role == GroupRole.Admin)
                throw new ForbiddenOperationException("Solo el Owner puede asignar rol Admin.");

            // (d) Verificar que el usuario a agregar existe [400 — error del caller, no 404]
            var memberUser = await _userRepository.GetByIdAsync(request.MemberUserId, cancellationToken)
                ?? throw new InvalidOperationException("El usuario especificado no existe en el sistema.");

            // (e) Añadir miembro; el dominio aplica la regla de duplicados [FR-007]
            group.AddMember(memberUser.Id, request.Role);

            await _groupRepository.UpdateAsync(group, cancellationToken);
        }
    }
}
