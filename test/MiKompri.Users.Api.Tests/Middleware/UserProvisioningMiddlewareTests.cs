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

    [Theory]
    [InlineData(null, null)]
    [InlineData("tenant-01", null)]
    [InlineData(null, "object-01")]
    public async Task GetMe_WithoutTidOidCorrelation_Returns401(string? tid, string? oid)
    {
        var client = _factory.CreateAuthenticatedClientWithoutCanonicalClaims(
            sub: "legacy-sub-02",
            name: "Bruno",
            email: "bruno@demo.com");

        if (tid is not null) client.DefaultRequestHeaders.Add(TestAuthHandler.TidHeaderName, tid);
        if (oid is not null) client.DefaultRequestHeaders.Add(TestAuthHandler.OidHeaderName, oid);

        var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, _factory.CountUsers());
    }

    [Fact]
    public async Task GetMe_WithIssuerAndOidButNoTid_Returns401WithoutProvisioning()
    {
        var client = _factory.CreateAuthenticatedClientWithoutCanonicalClaims("legacy-issuer-sub");
        client.DefaultRequestHeaders.Add(TestAuthHandler.OidHeaderName, "object-issuer");
        client.DefaultRequestHeaders.Add(TestAuthHandler.IssuerHeaderName, "https://login.microsoftonline.com/tenant-issuer/v2.0");

        var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, _factory.CountUsers());
    }

    [Fact]
    public async Task GetMe_WithMappedTidAndOid_ProvisionaUsuarioYRetorna200()
    {
        var client = _factory.CreateAuthenticatedClientWithoutCanonicalClaims("legacy-mapped-sub", "Ada", "ada@demo.com");
        client.DefaultRequestHeaders.Add(TestAuthHandler.MappedTidHeaderName, "tenant-mapped");
        client.DefaultRequestHeaders.Add(TestAuthHandler.MappedOidHeaderName, "object-mapped");

        var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, _factory.CountUsers());
    }
}
