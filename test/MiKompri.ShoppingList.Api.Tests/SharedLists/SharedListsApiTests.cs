using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.SharedLists
{
    public class SharedListsApiTests : IClassFixture<CustomWebApplicationFactory<ShoppingListApiProgram>>
    {
        private readonly CustomWebApplicationFactory<ShoppingListApiProgram> _factory;
        private readonly HttpClient _client;

        public SharedListsApiTests(CustomWebApplicationFactory<ShoppingListApiProgram> factory)
        {
            _factory = factory;
            _factory.UsersApiState.Reset();
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SharedEndpoints_ShouldReturn401_WhenTokenIsMissing()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SharedEndpoints_ShouldReturn403_WhenAuthenticatedButNotMember()
        {
            var ownerSub = "owner-shared-403";
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            var groupId = Guid.NewGuid();
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);
            var createResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Lista para 403",
                GroupId = groupId
            });
            createResponse.EnsureSuccessStatusCode();
            var sharedListId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "outsider-shared-403");

            var response = await _client.GetAsync($"/api/v1/shared-lists/{sharedListId}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PersonalEndpoints_ShouldStillWork_AsRegression()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var request = new CreatePurchaseListRequest
            {
                Name = "Lista personal",
                OwnerId = Guid.NewGuid()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/v1/PurchaseLists", request);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdId = await createResponse.Content.ReadFromJsonAsync<Guid>();
            var getResponse = await _client.GetAsync($"/api/v1/PurchaseLists/{createdId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }
    }
}
