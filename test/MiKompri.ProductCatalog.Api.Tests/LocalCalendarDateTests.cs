using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiKompri.ProductCatalog.Application.Dtos;

namespace MiKompri.ProductCatalog.Api.Tests;

public class LocalCalendarDateTests
{
    [Fact]
    public void PresentEmptyOrMultipleOffsetsAreRejected()
    {
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        var dates = new MiKompri.ProductCatalog.Api.Services.ClientDateTimeProvider(new Clock(), new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = context });
        context.Request.Headers["X-Client-Utc-Offset-Minutes"] = "";
        Assert.Throws<InvalidOperationException>(() => dates.Today);
        context.Request.Headers["X-Client-Utc-Offset-Minutes"] = new Microsoft.Extensions.Primitives.StringValues(new[] { "60", "120" });
        Assert.Throws<InvalidOperationException>(() => dates.Today);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2030, 10, 29, 23, 30, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public async Task PriceUsesCallerCivilDate_PreservesHistory_AndRejectsFutureAndInvalidOffsets()
    {
        var clock = new Clock();
        using var factory = new CustomWebApplicationFactory();
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => {
            services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(clock);
        }));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "calendar-009");
        var productResponse = await client.PostAsJsonAsync("/api/v1/catalog-products", new { name = "Calendario 009", purchaseUnit = "unidad" });
        productResponse.EnsureSuccessStatusCode();
        var product = (await productResponse.Content.ReadFromJsonAsync<CatalogProductDto>())!;
        var marketResponse = await client.PostAsJsonAsync("/api/v1/markets", new { name = "Mercado calendario" });
        marketResponse.EnsureSuccessStatusCode();
        var market = (await marketResponse.Content.ReadFromJsonAsync<MarketDto>())!;
        var date = new DateOnly(2030, 10, 30);
        var input = new { catalogProductId = product.Id, marketId = market.Id, effectiveDate = date, priceAmount = 1.25m };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/product-prices", input)).StatusCode); // Legacy UTC.
        client.DefaultRequestHeaders.Add("X-Client-Utc-Offset-Minutes", "60");
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/product-prices", input)).StatusCode);
        var history = (await client.GetFromJsonAsync<ProductPriceHistoryDto>($"/api/v1/catalog-products/{product.Id}/price-history"))!;
        Assert.Equal(date, Assert.Single(history.Records).EffectiveDate);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/product-prices", new { input.catalogProductId, input.marketId, effectiveDate = date.AddDays(1), input.priceAmount })).StatusCode);
        foreach (var offset in new[] { "841", "-841", "not-an-offset", "60,120" })
        {
            client.DefaultRequestHeaders.Remove("X-Client-Utc-Offset-Minutes");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Utc-Offset-Minutes", offset);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/product-prices", new { input.catalogProductId, input.marketId, effectiveDate = date.AddDays(-1), input.priceAmount })).StatusCode);
        }
        clock.Now = new DateTimeOffset(2030, 10, 30, 0, 30, 0, TimeSpan.Zero);
        client.DefaultRequestHeaders.Remove("X-Client-Utc-Offset-Minutes");
        client.DefaultRequestHeaders.Add("X-Client-Utc-Offset-Minutes", "-120");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/product-prices", input)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/product-prices", new { input.catalogProductId, input.marketId, effectiveDate = date.AddDays(-1), input.priceAmount })).StatusCode);
    }
}
