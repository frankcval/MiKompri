using System.Net;

namespace MiKompri.Users.Api.Tests.Middleware;

public class UserProvisioningMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserProvisioningMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task GetMe_WithTidAndOidClaims_ProvisionaUsuarioYRetorna200()
    {
        var client = _factory.CreateAuthenticatedClient(
            sub: "legacy-sub-01",
            name: "Ada",
            email: "ada@demo.com",
            tid: "tenant-01",
            oid: "object-01");

        var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithoutTidOidCorrelation_Returns401()
    {
        var client = _factory.CreateAuthenticatedClientWithoutCanonicalClaims(
            sub: "legacy-sub-02",
            name: "Bruno",
            email: "bruno@demo.com");

        var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
