using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetExpenses;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests;

public class PurchaseCompletionApiTests
{
    private static HttpClient Client(CustomWebApplicationFactory<ShoppingListApiProgram> factory, string user)
    {
        factory.UsersApiState.EnsureUser(user);
        var client = factory.CreateClient(); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", user); return client;
    }

    [Fact]
    public async Task PersonalPurchase_ReplaysWithoutDuplicates_ProtectsHistory_AndDetectsStaleCorrection()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var client = Client(factory, "personal-009");
        using var stranger = Client(factory, "stranger-009");
        var created = await client.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Compra personal" }); created.EnsureSuccessStatusCode();
        var list = await created.Content.ReadFromJsonAsync<Guid>();
        var added = await client.PostAsJsonAsync($"/api/v1/purchaselists/{list}/items", new { productId = Guid.NewGuid(), productName = "Pan", quantity = 2, price = 1 }); added.EnsureSuccessStatusCode();
        var item = await added.Content.ReadFromJsonAsync<Guid>();
        var url = $"/api/v1/purchaselists/{list}";
        var input = new { operationId = Guid.NewGuid(), realPaidPrice = 2.50m, currency = "EUR", marketId = Guid.NewGuid(), purchasedOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1) };
        var first = await client.PostAsJsonAsync($"{url}/items/{item}/purchases", input); first.EnsureSuccessStatusCode();
        var id = await first.Content.ReadFromJsonAsync<Guid>();
        var replay = await client.PostAsJsonAsync($"{url}/items/{item}/purchases", input); replay.EnsureSuccessStatusCode();
        Assert.Equal(id, await replay.Content.ReadFromJsonAsync<Guid>());
        var records = await client.GetFromJsonAsync<ExpenseView[]>($"{url}/purchases");
        var record = Assert.Single(records!);
        Assert.Equal(input.marketId, record.MarketId); Assert.Equal(input.purchasedOn, record.PurchasedOn); Assert.Empty(record.Participants);
        Assert.Equal(factory.UsersApiState.EnsureUser("personal-009"), record.PaidBy);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.GetAsync($"{url}/purchases")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.PostAsJsonAsync($"{url}/items/{item}/purchases", input)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"{url}/items/{item}/purchases", new { input.operationId, realPaidPrice = 9, currency = "EUR" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"{url}/items/{item}")).StatusCode);
        var correction = new { expectedVersion = record.Version, realPaidPrice = 3, currency = "EUR" };
        Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsJsonAsync($"{url}/items/{item}/purchases/{id}", correction)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PatchAsJsonAsync($"{url}/items/{item}/purchases/{id}", correction)).StatusCode);
        await client.PatchAsJsonAsync($"{url}/items/{item}/purchase-state", new { isPurchased = true });
        await client.PatchAsJsonAsync($"{url}/items/{item}/purchase-state", new { isPurchased = false });
        Assert.Single((await client.GetFromJsonAsync<ExpenseView[]>($"{url}/purchases"))!);
        Assert.False(Assert.Single((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items).IsPurchased);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{url}/items/{item}/purchases/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"{url}/items/{item}/purchases", input)).StatusCode);
    }

    [Fact]
    public async Task SharedCaptureAndExpensesUseSameRecord_WithMemberRestrictionsAndReplay()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var owner = Client(factory, "shared-009"); using var member = Client(factory, "member-009");
        var group = Guid.NewGuid(); var actor = factory.UsersApiState.EnsureUser("shared-009"); var other = factory.UsersApiState.EnsureUser("member-009");
        factory.UsersApiState.SetMembership(group, actor, "Owner"); factory.UsersApiState.SetMembership(group, other, "Member");
        var created = await owner.PostAsJsonAsync("/api/v1/shared-lists", new { name = "Compartida", groupId = group }); created.EnsureSuccessStatusCode(); var list = await created.Content.ReadFromJsonAsync<Guid>();
        var added = await owner.PostAsJsonAsync($"/api/v1/shared-lists/{list}/items", new { productId = Guid.NewGuid(), name = "Leche", quantity = 1, estimatedPrice = 2 }); added.EnsureSuccessStatusCode(); var item = await added.Content.ReadFromJsonAsync<Guid>();
        var url = $"/api/v1/shared-lists/{list}";
        var input = new { operationId = Guid.NewGuid(), realPaidPrice = 2.30m, currency = "EUR", paidBy = actor, purchasedBy = actor, participants = new[] { actor, other }, purchasedOn = DateOnly.FromDateTime(DateTime.UtcNow) };
        Assert.Equal(HttpStatusCode.Forbidden, (await member.PostAsJsonAsync($"{url}/items/{item}/expenses", input)).StatusCode);
        var first = await owner.PostAsJsonAsync($"{url}/items/{item}/expenses", input); first.EnsureSuccessStatusCode(); var id = await first.Content.ReadFromJsonAsync<Guid>();
        var replay = await owner.PostAsJsonAsync($"{url}/items/{item}/expenses", input); replay.EnsureSuccessStatusCode(); Assert.Equal(id, await replay.Content.ReadFromJsonAsync<Guid>());
        var record = Assert.Single((await owner.GetFromJsonAsync<ExpenseView[]>($"{url}/expenses"))!); Assert.Equal(id, record.ExpenseId);
        Assert.Equal(HttpStatusCode.NoContent, (await owner.PatchAsJsonAsync($"{url}/items/{item}/expenses/{id}", new { expectedVersion = record.Version, realPaidPrice = 4m, currency = "EUR", paidBy = actor, participants = new[] { actor } })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await owner.PatchAsJsonAsync($"{url}/items/{item}/expenses/{id}", new { expectedVersion = record.Version, realPaidPrice = 5m, currency = "EUR", paidBy = actor, participants = new[] { actor } })).StatusCode);
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(1.001)]
    public async Task PersonalCaptureRejectsInvalidAmounts(decimal amount)
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>(); using var client = Client(factory, "invalid-009");
        var created = await client.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Validaciones" }); var list = await created.Content.ReadFromJsonAsync<Guid>();
        var added = await client.PostAsJsonAsync($"/api/v1/purchaselists/{list}/items", new { productId = Guid.NewGuid(), productName = "Pan", quantity = 1, price = 1 }); var item = await added.Content.ReadFromJsonAsync<Guid>();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync($"/api/v1/purchaselists/{list}/items/{item}/purchases", new { operationId = Guid.NewGuid(), realPaidPrice = amount, currency = "EUR" })).StatusCode);
    }

    [Fact]
    public async Task DuplicateOperation_UniqueReceiptAlsoProtectsConcurrentRelationalRequests()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>(); using var client = Client(factory, "race-009");
        var created = await client.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Concurrente" }); var list = await created.Content.ReadFromJsonAsync<Guid>();
        var added = await client.PostAsJsonAsync($"/api/v1/purchaselists/{list}/items", new { productId = Guid.NewGuid(), productName = "Pan", quantity = 1, price = 1 }); var item = await added.Content.ReadFromJsonAsync<Guid>();
        var url = $"/api/v1/purchaselists/{list}"; var input = new { operationId = Guid.NewGuid(), realPaidPrice = 1, currency = "EUR" };
        // InMemory cannot enforce relational uniqueness; PostgreSQL exercises the actual race.
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MIKOMPRI_TEST_POSTGRES")))
        {
            var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => client.PostAsJsonAsync($"{url}/items/{item}/purchases", input)));
            Assert.All(results, result => Assert.Contains(result.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }));
        }
        var final = await client.PostAsJsonAsync($"{url}/items/{item}/purchases", input); final.EnsureSuccessStatusCode();
        Assert.Single((await client.GetFromJsonAsync<ExpenseView[]>($"{url}/purchases"))!);
    }
}
