using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest
{
    public sealed class FakeUsersApiState
    {
        private readonly Dictionary<string, Guid> _usersByExternalSub = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Guid, Dictionary<Guid, string>> _membershipsByGroup = new();
        private readonly List<string> _requests = new();
        private readonly object _sync = new();

        public Guid EnsureUser(string externalSub)
        {
            lock (_sync)
            {
                if (_usersByExternalSub.TryGetValue(externalSub, out var existing))
                {
                    return existing;
                }

                var userId = Guid.NewGuid();
                _usersByExternalSub[externalSub] = userId;
                return userId;
            }
        }

        public void SetMembership(Guid groupId, Guid userId, string role)
        {
            lock (_sync)
            {
                if (!_membershipsByGroup.TryGetValue(groupId, out var members))
                {
                    members = new Dictionary<Guid, string>();
                    _membershipsByGroup[groupId] = members;
                }

                members[userId] = role;
            }
        }

        public bool TryGetMembers(Guid groupId, out IReadOnlyCollection<(Guid UserId, string Role)> members)
        {
            lock (_sync)
            {
                if (_membershipsByGroup.TryGetValue(groupId, out var dict))
                {
                    members = dict.Select(x => (x.Key, x.Value)).ToList();
                    return true;
                }

                members = Array.Empty<(Guid, string)>();
                return false;
            }
        }

        public void RecordRequest(string path)
        {
            lock (_sync)
            {
                _requests.Add(path);
            }
        }

        public IReadOnlyCollection<string> GetRequests()
        {
            lock (_sync)
            {
                return _requests.ToList();
            }
        }

        public void Reset()
        {
            lock (_sync)
            {
                _usersByExternalSub.Clear();
                _membershipsByGroup.Clear();
                _requests.Clear();
            }
        }
    }

    public sealed class FakeUsersApiHandler : HttpMessageHandler
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly FakeUsersApiState _state;

        public FakeUsersApiHandler(FakeUsersApiState state)
        {
            _state = state;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            _state.RecordRequest(path);

            if (!TryGetExternalSub(request.Headers.Authorization, out var externalSub))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            var callerUserId = _state.EnsureUser(externalSub);

            if (HttpMethod.Get == request.Method && path.Equals("/api/v1/users/me", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(Json(HttpStatusCode.OK, new
                {
                    id = callerUserId,
                    displayName = externalSub,
                    identityProvider = "test",
                    externalUserId = externalSub
                }));
            }

            if (HttpMethod.Get == request.Method && path.StartsWith("/api/v1/groups/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/members", StringComparison.OrdinalIgnoreCase))
            {
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length != 5 || !Guid.TryParse(segments[3], out var groupId))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }

                if (!_state.TryGetMembers(groupId, out var members) || members.All(x => x.UserId != callerUserId))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden));
                }

                var payload = members.Select(x => new
                {
                    userId = x.UserId,
                    role = x.Role,
                    joinedAt = DateTime.UtcNow,
                    displayName = x.UserId.ToString()
                }).ToList();

                return Task.FromResult(Json(HttpStatusCode.OK, payload));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static bool TryGetExternalSub(AuthenticationHeaderValue? authorization, out string externalSub)
        {
            externalSub = string.Empty;
            if (authorization is null || string.IsNullOrWhiteSpace(authorization.Scheme) || string.IsNullOrWhiteSpace(authorization.Parameter))
            {
                return false;
            }

            externalSub = authorization.Parameter.Trim();
            return !string.IsNullOrWhiteSpace(externalSub);
        }

        private static HttpResponseMessage Json(HttpStatusCode statusCode, object payload)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
