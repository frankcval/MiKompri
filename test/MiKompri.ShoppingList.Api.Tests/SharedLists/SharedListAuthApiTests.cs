using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.SharedLists
{
    public class SharedListAuthApiTests : IClassFixture<CustomWebApplicationFactory<ShoppingListApiProgram>>
    {
        private readonly CustomWebApplicationFactory<ShoppingListApiProgram> _factory;
        private readonly HttpClient _client;

        public SharedListAuthApiTests(CustomWebApplicationFactory<ShoppingListApiProgram> factory)
        {
            _factory = factory;
            _factory.UsersApiState.Reset();
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SharedEndpoints_ShouldReturn401_WhenAuthorizationHeaderIsMissing()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SharedEndpoints_ShouldAccept_NonGuid_Sub_And_Call_UsersApi()
        {
            var ownerSub = "external-sub-owner";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);

            var createResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Auth matrix",
                GroupId = groupId
            });
            createResponse.EnsureSuccessStatusCode();
            var listId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var getResponse = await _client.GetAsync($"/api/v1/shared-lists/{listId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var requests = _factory.UsersApiState.GetRequests();
            Assert.Contains("/api/v1/users/me", requests);
            Assert.Contains($"/api/v1/groups/{groupId}/members", requests);
        }

        [Fact]
        public async Task SharedEndpoints_ShouldReturn403_WhenCallerIsAuthenticatedButNotGroupMember()
        {
            var ownerSub = "external-sub-owner";
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            var groupId = Guid.NewGuid();
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);

            var createResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Auth matrix",
                GroupId = groupId
            });
            createResponse.EnsureSuccessStatusCode();
            var listId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "external-sub-outsider");

            var response = await _client.GetAsync($"/api/v1/shared-lists/{listId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SharedEndpoints_ShouldReturn403_WhenUsersApiReturnsUnknownRole()
        {
            // Rol desconocido que no puede parsearse a GroupRole → fail-closed → 403
            var ownerSub = "unknown-role-sub";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "UNKNOWN_ROLE_XYZ");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);

            var createResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Unknown role list",
                GroupId = groupId
            });

            // La creación pasa porque el handler de Create también llama a GetMembership;
            // si el rol es desconocido, fail-closed devuelve 403.
            Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        }

        [Fact]
        public async Task SharedEndpoints_ShouldReturn503_WhenUsersApiReturns5xx()
        {
            var ownerSub = "owner-503-sub";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);

            // Crear la lista mientras el servicio está disponible
            var createResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "503 test list",
                GroupId = groupId
            });
            createResponse.EnsureSuccessStatusCode();
            var listId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Forzar error 500 de Users API para la siguiente llamada a membresía
            _factory.UsersApiState.SimulateHttpErrorForGroup(groupId, 500);

            var response = await _client.GetAsync($"/api/v1/shared-lists/{listId}");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
    }
}
