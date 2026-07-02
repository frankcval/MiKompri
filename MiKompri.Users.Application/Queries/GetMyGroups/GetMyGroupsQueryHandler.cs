using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Dtos;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Application.Queries.GetMyGroups
{
    public class GetMyGroupsQueryHandler
        : IRequestHandler<GetMyGroupsQuery, IReadOnlyCollection<GroupDto>>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;

        public GetMyGroupsQueryHandler(
            ICurrentUserService currentUser,
            IGroupRepository groupRepository,
            IUserRepository userRepository)
        {
            _currentUser = currentUser;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
        }

        public async Task<IReadOnlyCollection<GroupDto>> Handle(
            GetMyGroupsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                throw new InvalidOperationException("Usuario no autenticado.");

            var userId = _currentUser.UserId;

            var groups = await _groupRepository.GetByUserIdAsync(userId, cancellationToken);

            var result = new List<GroupDto>();

            foreach (var group in groups)
            {
                var membersDto = new List<GroupMemberDto>();

                foreach (var membership in group.Memberships)
                {
                    var memberUser = await _userRepository
                        .GetByIdAsync(membership.UserId, cancellationToken);

                    if (memberUser is null)
                        continue;

                    membersDto.Add(new GroupMemberDto
                    {
                        UserId = memberUser.Id,
                        DisplayName = memberUser.DisplayName,
                        Email = memberUser.Email,
                        Role = membership.Role.ToString(),
                        JoinedAt = membership.JoinedAt
                    });
                }

                result.Add(new GroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    OwnerId = group.OwnerId,
                    MyRole = group.GetMemberRole(userId)?.ToString() ?? string.Empty,
                    Members = membersDto
                });
            }

            return result;
        }
    }
}
