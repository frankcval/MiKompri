using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Domain.Abstractions;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Application.Commands.RemoveMemberFromGroup
{
    public class RemoveMemberFromGroupCommandHandler : IRequestHandler<RemoveMemberFromGroupCommand>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupRepository _groupRepository;

        public RemoveMemberFromGroupCommandHandler(
            ICurrentUserService currentUser,
            IGroupRepository groupRepository)
        {
            _currentUser = currentUser;
            _groupRepository = groupRepository;
        }

        public async Task Handle(
            RemoveMemberFromGroupCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;

            // (a) Grupo no encontrado → 403 para evitar enumeración de grupos [G1, SR-006]
            var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken)
                ?? throw new ForbiddenOperationException("Acceso denegado.");

            // (b) El caller debe ser miembro con rol Admin u Owner
            var callerRole = group.GetMemberRole(currentUserId);
            if (callerRole is null)
                throw new ForbiddenOperationException("El caller no es miembro del grupo.");
            if (callerRole == GroupRole.Member)
                throw new ForbiddenOperationException("Los miembros con rol Member no pueden eliminar miembros.");

            // (c) Reglas de negocio delegadas al agregado (FR-009, FR-010)
            group.RemoveMember(request.TargetUserId, callerRole.Value);

            // (d) Persistir
            await _groupRepository.UpdateAsync(group, cancellationToken);
        }
    }
}
