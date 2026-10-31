using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.Users.Infrastructure.Persistence;

namespace MiKompri.Users.Api.Tests;
public class AccountStatusApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public AccountStatusApiTests(CustomWebApplicationFactory factory) { _factory = factory; factory.ResetDatabase(); }
    [Fact]
    public async Task StatusRead_DoesNotProvisionAnUnknownIdentity()
    {
        var client = _factory.CreateAuthenticatedClient("status-unknown");
        var response = await client.GetAsync("/api/v1/users/me/access-status");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, _factory.CountUsers());
    }
    [Fact]
    public async Task Suspension_DeniesExistingTokenAndReactivationDoesNotReviveIt()
    {
        var client = _factory.CreateAuthenticatedClient("status-existing");
        Assert.True((await client.PostAsync("/api/v1/users/me/sync", null)).IsSuccessStatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            db.Users.Single().SetSuspended(true, DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/users/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/users/me/access-status")).StatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            db.Users.Single().SetSuspended(false, DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }
        // Test ticket has no iat; after a security cut it must fail closed.
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/users/me")).StatusCode);
    }
}
