using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiKompri.ProductCatalog.Application.Dtos;

namespace MiKompri.ProductCatalog.Api.Tests;

public class CatalogPriceFlowTests
{
    [Fact]
    public async Task ProductMarketPriceAndHistory_WorkAcrossHttpRequests_AndKeepInactiveMarketName()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "catalog-009");
        var create = await client.PostAsJsonAsync("/api/v1/catalog-products", new { name = "Pan 009", purchaseUnit = "unidad" }); create.EnsureSuccessStatusCode(); var product = (await create.Content.ReadFromJsonAsync<CatalogProductDto>())!;
        var marketResponse = await client.PostAsJsonAsync("/api/v1/markets", new { name = "Mercado 009", locationHint = "Centro" }); marketResponse.EnsureSuccessStatusCode(); var market = (await marketResponse.Content.ReadFromJsonAsync<MarketDto>())!;
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var input = new { catalogProductId = product.Id, marketId = market.Id, effectiveDate = date, priceAmount = 1.25m };
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/product-prices", input)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/product-prices", input)).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<CatalogProductDto[]>("/api/v1/catalog-products?search=Pan%20009"))!);
        var historyUrl = $"/api/v1/catalog-products/{product.Id}/price-history";
        var history = (await client.GetFromJsonAsync<ProductPriceHistoryDto>($"{historyUrl}?marketId={market.Id}&from={date:yyyy-MM-dd}&to={date:yyyy-MM-dd}"))!;
        var record = Assert.Single(history.Records); Assert.Equal(1.25m, record.PriceAmount); Assert.Equal("EUR", record.Currency); Assert.Equal(market.Name, record.MarketName);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsync($"/api/v1/markets/{market.Id}/deactivate", null)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<MarketDto[]>("/api/v1/markets"))!);
        Assert.Equal(market.Name, Assert.Single((await client.GetFromJsonAsync<ProductPriceHistoryDto>(historyUrl))!.Records).MarketName);
        Assert.Empty((await client.GetFromJsonAsync<ProductPriceHistoryDto>($"{historyUrl}?from={date.AddDays(1):yyyy-MM-dd}"))!.Records);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"{historyUrl}?from=2026-10-28&to=2026-01-01")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/v1/catalog-products/{product.Id}", new { name = "Pan editado", purchaseUnit = "unidad" })).StatusCode);
    }
}
