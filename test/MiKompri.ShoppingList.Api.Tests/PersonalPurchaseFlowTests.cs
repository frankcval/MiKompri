using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests;

public class PersonalPurchaseFlowTests
{
    [Fact]
    public async Task Create_Add_Read_Mark_Read_UsesTheReturnedItemId_AndPersistsProgress()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "purchase-flow");
        factory.UsersApiState.EnsureUser("purchase-flow");
        var created = await client.PostAsJsonAsync("/api/v1/purchaselists", new { name = "Compra de regresión" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var listId = await created.Content.ReadFromJsonAsync<Guid>();
        var productId = Guid.NewGuid();
        var added = await client.PostAsJsonAsync($"/api/v1/purchaselists/{listId}/items", new { productId, productName = "Arroz", quantity = 2, price = 1.25m });
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var itemId = await added.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(productId, itemId);
        var before = await client.GetFromJsonAsync<PurchaseListDTO[]> ("/api/v1/purchaselists/mine");
        var item = Assert.Single(before!.Single(list => list.Id == listId).Items);
        Assert.Equal(itemId, item.Id);
        Assert.False(item.IsPurchased);

        var marked = await client.PostAsync($"/api/v1/purchaselists/{listId}/items/{item.Id}/mark-as-purchased", null);
        Assert.Equal(HttpStatusCode.NoContent, marked.StatusCode);

        var after = await client.GetFromJsonAsync<PurchaseListDTO[]>("/api/v1/purchaselists/mine");
        var list = after!.Single(list => list.Id == listId);
        Assert.True(Assert.Single(list.Items).IsPurchased);
        Assert.Equal(1, list.PurchasedItems);
        Assert.Equal(0, list.PendingItems);
        Assert.Equal(100d, list.CompletionPercentage);
        var repeated = await client.PostAsync($"/api/v1/purchaselists/{listId}/items/{itemId}/mark-as-purchased", null);
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
    }
}
