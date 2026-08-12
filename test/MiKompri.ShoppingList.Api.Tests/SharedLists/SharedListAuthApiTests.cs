using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.SharedLists
{
    public class SharedListAuthApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SharedListAuthApiTests(CustomWebApplicationFactory<Program> factory)
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
    }
}
