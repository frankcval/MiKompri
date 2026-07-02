using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiKompri.Users.Api.Models;
using MiKompri.Users.Application.Commands.SyncProfile;
using MiKompri.Users.Application.Commands.UpdateProfile;
using MiKompri.Users.Application.Dtos;
using MiKompri.Users.Application.Queries.GetMyProfile;
using System.Security.Claims;

namespace MiKompri.Users.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IConfiguration _configuration;

        public ProfileController(ISender sender, IConfiguration configuration)
        {
            _sender = sender;
            _configuration = configuration;
        }

        // GET /api/v1/users/me
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile(CancellationToken ct)
        {
            var dto = await _sender.Send(new GetMyProfileQuery(), ct);
            return Ok(dto);
        }

        // PUT /api/v1/users/me
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile(
            [FromBody] UpdateProfileRequest request,
            CancellationToken ct)
        {
            var dto = await _sender.Send(new UpdateProfileCommand(request.DisplayName), ct);
            return Ok(dto);
        }

        // POST /api/v1/users/me/sync
        [HttpPost("me/sync")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> SyncProfile(CancellationToken ct)
        {
            // Leer claims del JWT; el middleware de autenticación ya los validó
            var sub = User.FindFirstValue("sub")
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? string.Empty;

            var displayName = User.FindFirstValue("name")
                              ?? User.FindFirstValue(ClaimTypes.Name);

            var emailClaim = User.FindFirstValue("email")
                             ?? User.FindFirstValue(ClaimTypes.Email);

            // email null si ausente — nunca cadena vacía [C4]
            var email = string.IsNullOrEmpty(emailClaim) ? null : emailClaim;

            var identityProvider = _configuration["Authentication:IdentityProvider"] ?? "entra";

            var command = new SyncProfileCommand(identityProvider, sub, displayName, email);
            var (_, created) = await _sender.Send(command, ct);

            var profile = await _sender.Send(new GetMyProfileQuery(), ct);

            return created
                ? CreatedAtAction(nameof(GetMyProfile), profile)
                : Ok(profile);
        }
    }
}
