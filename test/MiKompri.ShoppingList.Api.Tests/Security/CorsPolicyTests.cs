using System.Net;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;

namespace MiKompri.ShoppingList.Api.Tests.Security;

public class CorsPolicyTests : IClassFixture<CustomWebApplicationFactory<ShoppingListApiProgram>>
{
    private readonly HttpClient _client;

    public CorsPolicyTests(CustomWebApplicationFactory<ShoppingListApiProgram> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Preflight_WithAllowedOrigin_ShouldReturnCorsHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/purchaselists");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains("http://localhost:5173", values);
    }

    [Fact]
    public async Task Preflight_WithBlockedOrigin_ShouldNotReturnCorsHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/purchaselists");
        request.Headers.Add("Origin", "https://malicious.example");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False(response.Headers.TryGetValues("Access-Control-Allow-Origin", out _));
    }
}
