using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.SharedLists
{
    public class SettlementApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SettlementApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.UsersApiState.Reset();
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SettlementEndpoints_ShouldReturn401_WhenTokenMissing()
        {
            var response = await _client.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}/settlement/summary");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SettlementEndpoints_ShouldReturn403_WhenAuthenticatedButUnauthorized()
        {
            var ownerSub = "settlement-owner-sub";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);
            var createListResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "List for authz",
                GroupId = groupId
            });
            createListResponse.EnsureSuccessStatusCode();
            var listId = await createListResponse.Content.ReadFromJsonAsync<Guid>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "settlement-outsider-sub");
            var response = await _client.GetAsync($"/api/v1/shared-lists/{listId}/settlement/proposal");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SettlementProposal_ShouldBeDeterministic_ForSameData()
        {
            var ownerSub = "settlement-deterministic-owner";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);

            var createListResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Settlement list",
                GroupId = groupId
            });
            createListResponse.EnsureSuccessStatusCode();
            var listId = await createListResponse.Content.ReadFromJsonAsync<Guid>();

            var addItemResponse = await _client.PostAsJsonAsync($"/api/v1/shared-lists/{listId}/items", new AddSharedItemRequest
            {
                ProductId = Guid.NewGuid(),
                Name = "Pan",
                EstimatedPrice = 3m,
                Quantity = 1
            });
            addItemResponse.EnsureSuccessStatusCode();
            var itemId = await addItemResponse.Content.ReadFromJsonAsync<Guid>();

            var expenseResponse = await _client.PostAsJsonAsync($"/api/v1/shared-lists/{listId}/items/{itemId}/expenses", new RegisterExpenseRequest
            {
                PaidBy = ownerUserId,
                RealPaidPrice = 10m,
                Currency = "EUR",
                Participants = new List<Guid> { ownerUserId }
            });
            var expenseBody = await expenseResponse.Content.ReadAsStringAsync();
            Assert.True(expenseResponse.IsSuccessStatusCode, expenseBody);

            var first = await _client.GetFromJsonAsync<MiKompri.ShoppingList.Application.DTOs.SettlementProposalDto>($"/api/v1/shared-lists/{listId}/settlement/proposal");
            var second = await _client.GetFromJsonAsync<MiKompri.ShoppingList.Application.DTOs.SettlementProposalDto>($"/api/v1/shared-lists/{listId}/settlement/proposal");

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(first!.Transfers.Count, second!.Transfers.Count);
            Assert.Equal(first.Transfers.Select(x => (x.FromUserId, x.ToUserId, x.Amount)), second.Transfers.Select(x => (x.FromUserId, x.ToUserId, x.Amount)));
        }
    }
}
