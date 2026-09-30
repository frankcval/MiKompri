using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiKompri.ShoppingList.Application.Exceptions;
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
            try
            {
                using var response = await client.SendAsync(request, cancellationToken);
                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Users API rechazó el token al resolver usuario actual ({StatusCode}).", (int)response.StatusCode);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Users API devolvió error {StatusCode} al resolver usuario actual.", (int)response.StatusCode);
                    throw new DependencyUnavailableException(
                        $"Users API no disponible (HTTP {(int)response.StatusCode}) al resolver identidad.");
                }

                var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions, cancellationToken);
                return profile?.Id;
            }
            catch (DependencyUnavailableException)
            {
                throw;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout al llamar a Users API para resolver usuario actual.");
                throw new DependencyUnavailableException("Users API no respondió a tiempo al resolver identidad.", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al llamar a Users API para resolver usuario actual.");
                throw new DependencyUnavailableException("No se pudo conectar con Users API al resolver identidad.", ex);
            }
        }

        public async Task<GroupAuthorizationResult> GetMembershipAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, $"/api/v1/groups/{groupId}/members");
            if (request is null)
            {
                return new GroupAuthorizationResult(false, false, null);
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            try
            {
                using var response = await client.SendAsync(request, cancellationToken);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Users API rechazó el token al consultar membresía para grupo {GroupId} ({StatusCode}).", groupId, (int)response.StatusCode);
                    throw new ForbiddenOperationException("Acceso denegado al consultar membresía de grupo.");
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Grupo {GroupId} no encontrado en Users API.", groupId);
                    return new GroupAuthorizationResult(false, false, null);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Users API devolvió error {StatusCode} al consultar membresía para grupo {GroupId}.", (int)response.StatusCode, groupId);
                    throw new DependencyUnavailableException(
                        $"Users API no disponible (HTTP {(int)response.StatusCode}) al consultar membresía.");
                }

                var members = await response.Content.ReadFromJsonAsync<List<GroupMemberResponse>>(JsonOptions, cancellationToken)
                              ?? new List<GroupMemberResponse>();
                var member = members.FirstOrDefault(m => m.UserId == userId);
                if (member is null)
                {
                    return new GroupAuthorizationResult(false, false, null);
                }

                if (!Enum.TryParse<GroupRole>(member.Role, ignoreCase: true, out var role))
                {
                    // Rol desconocido → fail-closed: denegar acceso y registrar advertencia
                    _logger.LogWarning(
                        "Rol desconocido '{Role}' devuelto por Users API para usuario {UserId} en grupo {GroupId}. Denegando acceso (fail-closed).",
                        member.Role, userId, groupId);
                    return new GroupAuthorizationResult(false, false, null);
                }

                return new GroupAuthorizationResult(true, true, role);
            }
            catch (DependencyUnavailableException)
            {
                throw;
            }
            catch (ForbiddenOperationException)
            {
                throw;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout al llamar a Users API para consultar membresía del grupo {GroupId}.", groupId);
                throw new DependencyUnavailableException("Users API no respondió a tiempo al consultar membresía.", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al llamar a Users API para consultar membresía del grupo {GroupId}.", groupId);
                throw new DependencyUnavailableException("No se pudo conectar con Users API al consultar membresía.", ex);
            }
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
