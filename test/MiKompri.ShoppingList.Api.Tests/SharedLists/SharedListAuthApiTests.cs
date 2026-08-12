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
        private readonly HttpClient _client;

        public SharedListAuthApiTests(CustomWebApplicationFactory<Program> factory)
        {
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
        public async Task SharedEndpoints_ShouldReturn401_WhenSubClaimIsNotGuid()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "not-a-guid");

            var response = await _client.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SharedEndpoints_ShouldReturn403_WhenCallerIsAuthenticatedButNotGroupMember()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", TestGroupAuthorizationService.AllowedUserId.ToString());

            var createResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Auth matrix",
                GroupId = Guid.NewGuid()
            });
            createResponse.EnsureSuccessStatusCode();
            var listId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", Guid.NewGuid().ToString());

            var response = await _client.GetAsync($"/api/v1/shared-lists/{listId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
