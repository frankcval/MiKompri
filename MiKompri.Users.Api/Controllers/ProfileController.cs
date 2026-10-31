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

        [HttpGet("me/access-status")]
        [ProducesResponseType(typeof(MiKompri.Identity.AccessStatus), StatusCodes.Status200OK)]
        public ActionResult<MiKompri.Identity.AccessStatus> GetAccessStatus()
        {
            Response.Headers.CacheControl = "no-store";
            return HttpContext.Items["MiKompri.AccessStatus"] is MiKompri.Identity.AccessStatus status
                ? Ok(status) : StatusCode(503);
        }

        // GET /api/v1/users/me
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile(CancellationToken ct)
        {
            var dto = await _sender.Send(new GetMyProfileQuery(), ct);
            Response.Headers.ETag = $"\"{dto.ProfileVersion}\"";
            Response.Headers.CacheControl = "no-store";
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
            var fields = request.Fields ?? new Dictionary<string, System.Text.Json.JsonElement>();
            var allowed = new[] { "username", "givenName", "familyName", "preferredLanguage", "defaultListSort" };
            if (fields.Keys.Any(key => !allowed.Contains(key, StringComparer.Ordinal)))
                return BadRequest(new { status = 400, error = "Campo de perfil no permitido." });
            long? version = null;
            var match = Request.Headers.IfMatch.ToString();
            if (fields.Count > 0 && string.IsNullOrWhiteSpace(match))
                return StatusCode(428, new { status = 428, error = "Actualiza el perfil antes de guardar." });
            if (!string.IsNullOrEmpty(match))
            {
                if (!long.TryParse(match.Trim('"'), out var parsed) || parsed < 0) return BadRequest();
                version = parsed;
            }
            string? Read(string field)
            {
                if (!fields.TryGetValue(field, out var value) || value.ValueKind == System.Text.Json.JsonValueKind.Null) return null;
                if (value.ValueKind != System.Text.Json.JsonValueKind.String) throw new InvalidOperationException("Campo de perfil no válido.");
                return value.GetString();
            }
            var details = fields.Count == 0 ? null : new MiKompri.Users.Domain.Users.ProfileDetails(request.DisplayName,
                Read("username"), Read("givenName"), Read("familyName"), Read("preferredLanguage"), Read("defaultListSort") ?? "");
            var dto = await _sender.Send(new UpdateProfileCommand(request.DisplayName, details, fields.Keys.ToHashSet(StringComparer.Ordinal), version), ct);
            Response.Headers.ETag = $"\"{dto.ProfileVersion}\"";
            Response.Headers.CacheControl = "no-store";
            return Ok(dto);
        }

        // POST /api/v1/users/me/sync
        [HttpPost("me/sync")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> SyncProfile()
        {
            var ct = HttpContext.RequestAborted;
            // Middleware has already resolved/provisioned exactly once under its policy.
            var createdByMiddleware = HttpContext.Items.TryGetValue("ProfileCreatedInMiddleware", out var middlewareValue)
                && middlewareValue is bool created
                && created;

            var profile = await _sender.Send(new GetMyProfileQuery(), ct);

            return createdByMiddleware
                ? CreatedAtAction(nameof(GetMyProfile), profile)
                : Ok(profile);
        }
    }
}
