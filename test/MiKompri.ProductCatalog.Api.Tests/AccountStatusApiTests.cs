using System.Net;
using System.Net.Http.Headers;
using MiKompri.ProductCatalog.Api.Tests;

namespace MiKompri.ProductCatalog.Api.Tests;
public class AccountStatusApiTests
{
    [Theory]
    [InlineData(200, false, 200)]
    [InlineData(200, true, 403)]
    [InlineData(201, false, 503)]
    [InlineData(202, false, 503)]
    [InlineData(401, false, 401)]
    [InlineData(403, false, 403)]
    [InlineData(500, false, 503)]
    [InlineData(503, false, 503)]
    public async Task CatalogGet_AlwaysChecksAuthoritativeState(int status, bool suspended, int expected)
    {
        using var app = new CustomWebApplicationFactory();
        app.AccountStatus.HttpStatus = status; app.AccountStatus.Suspended = suspended;
        using var client = app.CreateClient(); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");
        var response = await client.GetAsync("/api/v1/catalog-products");
        Assert.Equal(expected, (int)response.StatusCode); Assert.Equal(1, app.AccountStatus.Calls);
    }
    [Fact]
    public async Task MutationsFailClosed_AndASecondRequestCannotReuseOldAllow()
    {
        using var app = new CustomWebApplicationFactory();
        using var client = app.CreateClient(); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/catalog-products")).StatusCode);
        app.AccountStatus.ThrowNetwork = true;
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.PostAsync("/api/v1/catalog-products", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"))).StatusCode);
        Assert.Equal(2, app.AccountStatus.Calls);
    }
    [Fact]
    public async Task IncompleteState_IsNotAnImplicitActiveAccount()
    {
        using var app = new CustomWebApplicationFactory(); app.AccountStatus.Malformed = true;
        using var client = app.CreateClient(); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/api/v1/markets")).StatusCode);
    }
}
