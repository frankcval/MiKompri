using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.SharedLists
{
    public class SharedListExpensesApiTests : IClassFixture<CustomWebApplicationFactory<ShoppingListApiProgram>>
    {
        private readonly CustomWebApplicationFactory<ShoppingListApiProgram> _factory;
        private readonly HttpClient _client;

        public SharedListExpensesApiTests(CustomWebApplicationFactory<ShoppingListApiProgram> factory)
        {
            _factory = factory;
            _factory.UsersApiState.Reset();
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterExpense_ShouldReturn401_WhenTokenIsMissing()
        {
            var response = await _client.PostAsJsonAsync($"/api/v1/shared-lists/{Guid.NewGuid()}/items/{Guid.NewGuid()}/expenses",
                new RegisterExpenseRequest
                {
                    PaidBy = Guid.NewGuid(),
                    RealPaidPrice = 10m,
                    Participants = new List<Guid> { Guid.NewGuid() }
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RegisterExpense_ShouldReturn403_WhenAuthenticatedButNotMember()
        {
            var ownerSub = "expense-owner-sub";
            var groupId = Guid.NewGuid();
            var ownerUserId = _factory.UsersApiState.EnsureUser(ownerSub);
            _factory.UsersApiState.SetMembership(groupId, ownerUserId, "Owner");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", ownerSub);
            var createResponse = await _client.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "Shared candidate",
                GroupId = groupId
            });
            createResponse.EnsureSuccessStatusCode();
            var sharedListId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "expense-outsider-sub");

            var response = await _client.PostAsJsonAsync($"/api/v1/shared-lists/{sharedListId}/items/{Guid.NewGuid()}/expenses",
                new RegisterExpenseRequest
                {
                    PaidBy = Guid.NewGuid(),
                    RealPaidPrice = 10m,
                    Participants = new List<Guid> { Guid.NewGuid() }
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
