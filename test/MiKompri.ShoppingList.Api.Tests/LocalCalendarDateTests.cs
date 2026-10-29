using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetExpenses;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests;

public class LocalCalendarDateTests
{
    [Fact]
    public void PresentEmptyOrMultipleOffsetsAreRejected()
    {
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        var dates = new MiKompri.ShoppingList.Api.Services.ClientCurrentDateProvider(new Clock(), new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = context });
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PurchaseAndCorrectionUseCallerToday_WithoutConvertingStoredDate(bool shared)
    {
        var clock = new Clock();
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        var actor = factory.UsersApiState.EnsureUser("calendar-009");
        var group = Guid.NewGuid();
        factory.UsersApiState.SetMembership(group, actor, "Owner");
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => {
            services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(clock);
        }));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "calendar-009");
        var root = shared ? "/api/v1/shared-lists" : "/api/v1/purchaselists";
        object creation = shared ? new { name = "Fecha local QA", groupId = group } : new { name = "Fecha local QA" };
        var created = await client.PostAsJsonAsync(root, creation);
        created.EnsureSuccessStatusCode();
        var list = await created.Content.ReadFromJsonAsync<Guid>();
        var added = await client.PostAsJsonAsync($"{root}/{list}/items", new { productId = Guid.NewGuid(), productName = "Pan", name = "Pan", quantity = 1, price = 1m, estimatedPrice = 1m });
        added.EnsureSuccessStatusCode();
        var item = await added.Content.ReadFromJsonAsync<Guid>();
        var segment = shared ? "expenses" : "purchases";
        var endpoint = $"{root}/{list}/items/{item}/{segment}";
        var date = new DateOnly(2030, 10, 30);
        object Input(DateOnly selected) => new { operationId = Guid.NewGuid(), realPaidPrice = 2m, currency = "EUR", paidBy = actor, participants = new[] { actor }, purchasedOn = selected };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(endpoint, Input(date))).StatusCode);
        client.DefaultRequestHeaders.Add("X-Client-Utc-Offset-Minutes", "60");
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(endpoint, Input(date))).StatusCode);
        var record = Assert.Single((await client.GetFromJsonAsync<ExpenseView[]>($"{root}/{list}/{segment}"))!);
        Assert.Equal(date, record.PurchasedOn);
        var correction = new { expectedVersion = record.Version, realPaidPrice = 2.10m, currency = "EUR", paidBy = actor, participants = new[] { actor }, purchasedOn = date };
        Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsJsonAsync($"{endpoint}/{record.ExpenseId}", correction)).StatusCode);
        Assert.Equal(date, Assert.Single((await client.GetFromJsonAsync<ExpenseView[]>($"{root}/{list}/{segment}"))!).PurchasedOn);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(endpoint, Input(date.AddDays(1)))).StatusCode);
        foreach (var invalid in new[] { "841", "-841", "invalid", "60,120" })
        {
            client.DefaultRequestHeaders.Remove("X-Client-Utc-Offset-Minutes");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Utc-Offset-Minutes", invalid);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(endpoint, Input(date.AddDays(-1)))).StatusCode);
        }
        Assert.Single((await client.GetFromJsonAsync<ExpenseView[]>($"{root}/{list}/{segment}"))!);
        clock.Now = new DateTimeOffset(2030, 10, 30, 0, 30, 0, TimeSpan.Zero);
        client.DefaultRequestHeaders.Remove("X-Client-Utc-Offset-Minutes");
        client.DefaultRequestHeaders.Add("X-Client-Utc-Offset-Minutes", "-120");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(endpoint, Input(date))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(endpoint, Input(date.AddDays(-1)))).StatusCode);
    }
}
