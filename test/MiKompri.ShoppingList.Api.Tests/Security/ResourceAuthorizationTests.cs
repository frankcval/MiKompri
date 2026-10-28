using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.Security;

public class ResourceAuthorizationTests
{
    private static HttpClient Client(CustomWebApplicationFactory<ShoppingListApiProgram> factory, string user)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", user);
        factory.UsersApiState.EnsureUser(user);
        return client;
    }

    [Fact]
    public async Task ForeignOwnerCannotReadEditDeleteOrMark_AndAnonymousCannotRead()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var owner = Client(factory, "owner");
        using var stranger = Client(factory, "stranger");
        var created = await owner.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Privada" });
        created.EnsureSuccessStatusCode();
        var listId = await created.Content.ReadFromJsonAsync<Guid>();
        var added = await owner.PostAsJsonAsync($"/api/v1/purchaselists/{listId}/items", new { productId = Guid.NewGuid(), productName = "Pan", quantity = 1, price = 2 });
        added.EnsureSuccessStatusCode();
        var itemId = await added.Content.ReadFromJsonAsync<Guid>();
        var url = $"/api/v1/purchaselists/{listId}";
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.PutAsJsonAsync(url, new { name = "Robada" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.DeleteAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.PostAsync($"{url}/items/{itemId}/mark-as-purchased", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.DeleteAsync($"{url}/items/{itemId}")).StatusCode);
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(url)).StatusCode);
        var actual = await owner.GetFromJsonAsync<PurchaseListDTO>(url);
        Assert.Equal("Privada", actual!.Name);
        Assert.False(Assert.Single(actual.Items).IsPurchased);
    }

    [Fact]
    public async Task ExplicitPurchaseStateIsReversible_AndItemIdsAreNotProductIds()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var client = Client(factory, "buyer");
        var create = await client.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Compra" });
        var listId = await create.Content.ReadFromJsonAsync<Guid>();
        var productId = Guid.NewGuid();
        var add = await client.PostAsJsonAsync($"/api/v1/purchaselists/{listId}/items", new { productId, productName = "Arroz", quantity = 1 });
        var itemId = await add.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(productId, itemId);
        var url = $"/api/v1/purchaselists/{listId}";
        Assert.Null(Assert.Single((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items).ProductPrice);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PatchAsJsonAsync($"{url}/items/{itemId}/purchase-state", new { })).StatusCode);
        foreach (var state in new[] { true, true, false })
        {
            Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsJsonAsync($"{url}/items/{itemId}/purchase-state", new { isPurchased = state })).StatusCode);
            Assert.Equal(state, Assert.Single((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items).IsPurchased);
        }
        var edit = await client.PutAsJsonAsync($"{url}/items/{itemId}", new { productName = "Arroz integral", quantity = 3, price = 4 });
        Assert.True(edit.StatusCode == HttpStatusCode.NoContent, await edit.Content.ReadAsStringAsync());
        var updated = Assert.Single((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items);
        Assert.Equal(3, updated.Quantity);
        Assert.Equal("Arroz integral", updated.ProductName);
        Assert.Equal(4m, updated.ProductPrice);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"{url}/items/{itemId}", new { quantity = 4 })).StatusCode);
        Assert.Equal(4m, Assert.Single((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items).ProductPrice);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"{url}/items/{itemId}", new { price = (decimal?)null })).StatusCode);
        Assert.Null(Assert.Single((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items).ProductPrice);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"{url}/items/{itemId}", new { price = 0m })).StatusCode);
        Assert.Equal(0m, Assert.Single((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items).ProductPrice);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{url}/items/{itemId}")).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<PurchaseListDTO>(url))!.Items);
    }

    [Fact]
    public async Task SharedStateChecksMembershipAndClosedLists()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var owner = Client(factory, "group-owner");
        using var member = Client(factory, "group-member");
        using var outsider = Client(factory, "outsider");
        var groupId = Guid.NewGuid();
        factory.UsersApiState.SetMembership(groupId, factory.UsersApiState.EnsureUser("group-owner"), "Owner");
        factory.UsersApiState.SetMembership(groupId, factory.UsersApiState.EnsureUser("group-member"), "Member");
        Assert.Equal(HttpStatusCode.Forbidden, (await outsider.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Intrusión", groupId })).StatusCode);
        var legacyAuthorized = await owner.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Compartida compatible", groupId });
        legacyAuthorized.EnsureSuccessStatusCode();
        var create = await owner.PostAsJsonAsync("/api/v1/shared-lists", new { name = "Casa", groupId });
        create.EnsureSuccessStatusCode();
        var listId = await create.Content.ReadFromJsonAsync<Guid>();
        var add = await member.PostAsJsonAsync($"/api/v1/shared-lists/{listId}/items", new { productId = Guid.NewGuid(), name = "Leche", quantity = 1 });
        add.EnsureSuccessStatusCode();
        var itemId = await add.Content.ReadFromJsonAsync<Guid>();
        var stateUrl = $"/api/v1/purchaselists/{listId}/items/{itemId}/purchase-state";
        Assert.Equal(HttpStatusCode.NoContent, (await member.PatchAsJsonAsync(stateUrl, new { isPurchased = true })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await outsider.PatchAsJsonAsync(stateUrl, new { isPurchased = false })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await member.DeleteAsync($"/api/v1/shared-lists/{listId}")).StatusCode);
        var ownerId = factory.UsersApiState.EnsureUser("group-owner");
        var memberId = factory.UsersApiState.EnsureUser("group-member");
        var sharedUrl = $"/api/v1/shared-lists/{listId}";
        var paid = await owner.PostAsJsonAsync($"{sharedUrl}/items/{itemId}/expenses", new { realPaidPrice = 10, paidBy = ownerId, purchasedBy = ownerId, participants = new[] { ownerId, memberId }, currency = "EUR" });
        paid.EnsureSuccessStatusCode();
        var expenseId = await paid.Content.ReadFromJsonAsync<Guid>();
        var balances = await owner.GetStringAsync($"{sharedUrl}/settlement/summary");
        Assert.Equal(HttpStatusCode.NoContent, (await owner.PatchAsJsonAsync(stateUrl, new { isPurchased = false })).StatusCode);
        Assert.Equal(balances, await owner.GetStringAsync($"{sharedUrl}/settlement/summary"));
        var expenses = await owner.GetFromJsonAsync<MiKompri.ShoppingList.Application.Queries.SharedLists.GetExpenses.ExpenseView[]>($"{sharedUrl}/expenses");
        Assert.Equal(ownerId, Assert.Single(expenses!).CreatedBy);
        Assert.Equal(HttpStatusCode.Conflict, (await owner.DeleteAsync($"{sharedUrl}/items/{itemId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await owner.DeleteAsync(sharedUrl)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await member.DeleteAsync($"{sharedUrl}/items/{itemId}/expenses/{expenseId}")).StatusCode);
        factory.UsersApiState.RemoveMembership(groupId, factory.UsersApiState.EnsureUser("group-member"));
        Assert.Equal(HttpStatusCode.Forbidden, (await member.PatchAsJsonAsync(stateUrl, new { isPurchased = false })).StatusCode);
        factory.UsersApiState.SetMembership(groupId, factory.UsersApiState.EnsureUser("group-member"), "Member");
        Assert.Equal(HttpStatusCode.NoContent, (await owner.PatchAsync($"/api/v1/shared-lists/{listId}/close", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await member.PatchAsJsonAsync(stateUrl, new { isPurchased = false })).StatusCode);
    }
}
