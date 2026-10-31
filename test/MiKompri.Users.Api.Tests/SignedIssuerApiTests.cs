using System.Net.Http.Headers;
using MiKompri.Identity.TestSupport;
namespace MiKompri.Users.Api.Tests;
public class SignedIssuerApiTests
{
    [Theory]
    [InlineData("legacy", 200)]
    [InlineData("ciam", 200)]
    [InlineData("malformed", 401)]
    [InlineData("future-nbf", 401)]
    [InlineData("missing-oid", 401)]
    [InlineData("wrong-client", 401)]
    [InlineData("unsigned", 401)]
    [InlineData("hs256", 401)]
    [InlineData("wrong-key", 401)]
    [InlineData("google", 401)]
    [InlineData("graph", 401)]
    [InlineData("id", 401)]
    [InlineData("unknown-issuer", 401)]
    [InlineData("wrong-tenant", 401)]
    [InlineData("wrong-audience", 401)]
    [InlineData("missing-scope", 403)]
    [InlineData("wrong-scope", 403)]
    [InlineData("expired", 401)]
    [InlineData("old-new", 401)]
    [InlineData("new-old", 401)]
    public async Task ActualApi_ValidatesIssuerBoundTrust(string scenario, int expected)
    {
        using var factory = new CustomWebApplicationFactory();
        using var app = factory.WithWebHostBuilder(SignedAuthentication.Configure);
        using var client = app.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SignedAuthentication.Token(scenario));
        var response = await client.PostAsync("/api/v1/users/me/sync", null);
        Assert.Equal(expected, response.IsSuccessStatusCode ? 200 : (int)response.StatusCode);
    }
}
