using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.SharedLists
{
    public class SharedListExpensesApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SharedListExpensesApiTests(CustomWebApplicationFactory<Program> factory)
        {
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
            var createRequest = new CreatePurchaseListRequest
            {
                Name = "Shared candidate",
                OwnerId = Guid.NewGuid(),
                GroupId = Guid.NewGuid()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createRequest);
            createResponse.EnsureSuccessStatusCode();
            var sharedListId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", Guid.NewGuid().ToString());

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
