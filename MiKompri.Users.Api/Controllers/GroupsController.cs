using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiKompri.Users.Api.Models;
using MiKompri.Users.Application.Commands.AddMemberToGroup;
using MiKompri.Users.Application.Commands.CreateGroup;
using MiKompri.Users.Application.Commands.RemoveMemberFromGroup;
using MiKompri.Users.Application.Dtos;
using MiKompri.Users.Application.Queries.GetGroupMembers;
using MiKompri.Users.Application.Queries.GetMyGroups;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Api.Controllers
{
    [ApiController]
    [Route("api/v1/groups")]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly ISender _sender;

        public GroupsController(ISender sender)
        {
            _sender = sender;
        }

        // POST /api/v1/groups
        [HttpPost]
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GroupDto>> CreateGroup(
            [FromBody] CreateGroupRequest request,
            CancellationToken ct)
        {
            var groupId = await _sender.Send(new CreateGroupCommand(request.Name), ct);

            var groups = await _sender.Send(new GetMyGroupsQuery(), ct);
            var createdGroup = groups.First(g => g.Id == groupId);

            return Created($"/api/v1/groups/{groupId}/members", createdGroup);
        }

        // GET /api/v1/groups
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<GroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyCollection<GroupDto>>> GetMyGroups(CancellationToken ct)
        {
            var groups = await _sender.Send(new GetMyGroupsQuery(), ct);
            return Ok(groups);
        }

        // GET /api/v1/groups/{groupId}/members
        [HttpGet("{groupId:guid}/members")]
        [ProducesResponseType(typeof(IReadOnlyCollection<GroupMemberDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IReadOnlyCollection<GroupMemberDto>>> GetMembers(
            Guid groupId,
            CancellationToken ct)
        {
            var members = await _sender.Send(new GetGroupMembersQuery(groupId), ct);
            return Ok(members);
        }

        // POST /api/v1/groups/{groupId}/members
        [HttpPost("{groupId:guid}/members")]
        [ProducesResponseType(typeof(GroupMemberDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GroupMemberDto>> AddMember(
            Guid groupId,
            [FromBody] AddGroupMemberRequest request,
            CancellationToken ct)
        {
            if (!Enum.TryParse<GroupRole>(request.Role, ignoreCase: true, out var role) ||
                role is not (GroupRole.Member or GroupRole.Admin))
            {
                return BadRequest(new
                {
                    error = $"Rol '{request.Role}' no válido. Valores permitidos: Member, Admin."
                });
            }

            var command = new AddMemberToGroupCommand(groupId, request.UserId, role);
            await _sender.Send(command, ct);

            var members = await _sender.Send(new GetGroupMembersQuery(groupId), ct);
            var added = members.First(m => m.UserId == request.UserId);

            return CreatedAtAction(nameof(GetMembers), new { groupId }, added);
        }

        // DELETE /api/v1/groups/{groupId}/members/{userId}
        // Sc2: elimina la membresía del usuario, NO el grupo completo.
        [HttpDelete("{groupId:guid}/members/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMember(
            Guid groupId,
            Guid userId,
            CancellationToken ct)
        {
            await _sender.Send(new RemoveMemberFromGroupCommand(groupId, userId), ct);
            return NoContent();
        }
    }
}
