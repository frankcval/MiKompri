using MediatR;
using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Dtos;
using MiKompri.Users.Domain.Abstractions;
using MiKompri.Users.Domain.Users;
using MiKompri.Users.Infrastructure.Persistence;

namespace MiKompri.Users.Application.Queries.GetGroupMembers
{
    public class GetGroupMembersQueryHandler
        : IRequestHandler<GetGroupMembersQuery, IReadOnlyCollection<GroupMemberDto>>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupRepository _groupRepository;
        private readonly UsersDbContext _context;

        public GetGroupMembersQueryHandler(
            ICurrentUserService currentUser,
            IGroupRepository groupRepository,
            UsersDbContext context)
        {
            _currentUser = currentUser;
            _groupRepository = groupRepository;
            _context = context;
        }

        public async Task<IReadOnlyCollection<GroupMemberDto>> Handle(
            GetGroupMembersQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;

            // (a) Grupo no encontrado → 403 para evitar enumeración [G1, SR-006]
            var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken)
                ?? throw new ForbiddenOperationException("Acceso denegado.");

            // (b) El caller debe ser miembro del grupo
            var callerRole = group.GetMemberRole(currentUserId);
            if (callerRole is null)
                throw new ForbiddenOperationException("El caller no es miembro del grupo.");

            // (c) Anti-N+1: un solo roundtrip para todos los perfiles [G3]
            var memberIds = group.Memberships.Select(m => m.UserId).ToList();
            var usersById = await _context.Users
                .Where(u => memberIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            // (d) Mapear membresías a DTO con DisplayName y JoinedAt [FR-011]
            var result = group.Memberships
                .Where(m => usersById.ContainsKey(m.UserId))
                .Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    DisplayName = usersById[m.UserId].DisplayName,
                    Role = m.Role.ToString(),
                    JoinedAt = m.JoinedAt
                })
                .ToList();

            return result;
        }
    }
}
