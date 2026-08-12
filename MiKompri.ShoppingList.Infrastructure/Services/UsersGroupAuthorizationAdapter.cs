using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Infrastructure.Services
{
    public sealed class UsersGroupAuthorizationAdapter : IGroupAuthorizationService, IUserIdentityResolver
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UsersGroupAuthorizationAdapter> _logger;

        public UsersGroupAuthorizationAdapter(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UsersGroupAuthorizationAdapter> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Guid?> ResolveCurrentUserIdAsync(CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, "/api/v1/users/me");
            if (request is null)
            {
                return null;
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Users API devolvió {StatusCode} al resolver usuario actual.", (int)response.StatusCode);
                return null;
            }

            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions, cancellationToken);
            return profile?.Id;
        }

        public async Task<GroupAuthorizationResult> GetMembershipAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, $"/api/v1/groups/{groupId}/members");
            if (request is null)
            {
                return new GroupAuthorizationResult(false, false, null);
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Users API devolvió {StatusCode} consultando membresía para grupo {GroupId}.", (int)response.StatusCode, groupId);
                return new GroupAuthorizationResult(false, false, null);
            }

            var members = await response.Content.ReadFromJsonAsync<List<GroupMemberResponse>>(JsonOptions, cancellationToken) ?? new List<GroupMemberResponse>();
            var member = members.FirstOrDefault(m => m.UserId == userId);
            if (member is null)
            {
                return new GroupAuthorizationResult(false, false, null);
            }

            if (!Enum.TryParse<GroupRole>(member.Role, ignoreCase: true, out var role))
            {
                return new GroupAuthorizationResult(true, true, null);
            }

            return new GroupAuthorizationResult(true, true, role);
        }

        private HttpRequestMessage? CreateRequest(HttpMethod method, string relativePath)
        {
            var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return null;
            }

            var request = new HttpRequestMessage(method, relativePath);
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
            return request;
        }

        private sealed record UserProfileResponse(Guid Id);
        private sealed record GroupMemberResponse(Guid UserId, string Role);
    }
}
