using System.Net;
using System.Net.Http.Headers;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.Middleware;

public class UserProvisioningMiddlewareTests : IClassFixture<CustomWebApplicationFactory<ShoppingListApiProgram>>
{
    private readonly CustomWebApplicationFactory<ShoppingListApiProgram> _factory;

    public UserProvisioningMiddlewareTests(CustomWebApplicationFactory<ShoppingListApiProgram> factory)
    {
        _factory = factory;
        _factory.UsersApiState.Reset();
    }

    [Fact]
    public async Task SharedEndpoint_WithTidOidClaims_ShouldPassMiddlewareAndReachAuthorizationLayer()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "sub-middleware-ok");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.TidHeaderName, "tenant-01");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.OidHeaderName, "object-01");

        var response = await client.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SharedEndpoint_WithoutCanonicalCorrelation_ShouldReturn401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", string.Empty);

        var response = await client.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
