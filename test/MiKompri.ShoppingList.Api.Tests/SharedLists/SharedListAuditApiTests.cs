using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.SharedLists
{
    public class SharedListAuditApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SharedListAuditApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.UsersApiState.Reset();
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task AuditEndpoint_ShouldReturn401_WhenTokenMissing()
        {
            var response = await _client.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}/audit-events");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuditEndpoint_ShouldReturn403_WhenUserIsNotActiveMember()
        {
            var ownerSub = "audit-owner-sub";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);

            var createList = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Audit list",
                GroupId = groupId
            });
            createList.EnsureSuccessStatusCode();
            var listId = await createList.Content.ReadFromJsonAsync<Guid>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "audit-outsider-sub");
            var response = await _client.GetAsync($"/api/v1/shared-lists/{listId}/audit-events");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AuditEndpoint_ShouldReturnEvents_WhenUserIsAuthorized()
        {
            var ownerSub = "audit-authorized-owner";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);

            var createList = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Audit list ok",
                GroupId = groupId
            });
            createList.EnsureSuccessStatusCode();
            var listId = await createList.Content.ReadFromJsonAsync<Guid>();

            var addItem = await _client.PostAsJsonAsync($"/api/v1/shared-lists/{listId}/items", new AddSharedItemRequest
            {
                ProductId = Guid.NewGuid(),
                Name = "Huevos",
                EstimatedPrice = 5,
                Quantity = 1
            });
            addItem.EnsureSuccessStatusCode();

            var response = await _client.GetFromJsonAsync<List<MiKompri.ShoppingList.Application.DTOs.SharedListAuditEventDto>>($"/api/v1/shared-lists/{listId}/audit-events");
            Assert.NotNull(response);
            Assert.True(response!.Count >= 1);
            Assert.Contains(response, x => x.ActionType == "ItemAdded");
        }
    }
}
