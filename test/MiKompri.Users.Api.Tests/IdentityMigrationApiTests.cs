using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.Users.Api.Tests;

public class IdentityMigrationApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IdentityMigrationApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task LegacyUser_Authenticates_KeepsSameUserId_AndCapturesTidOid()
    {
        var legacyId = _factory.SeedLegacyUser("sub-legacy", "Ana", "ana@demo.com");
        var client = _factory.CreateAuthenticatedClient("sub-legacy", "Ana", "ana@demo.com", "tid-1", "oid-1");

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        profile!.Id.Should().Be(legacyId);

        var stored = _factory.FindUser(legacyId);
        stored!.TenantId.Should().Be("tid-1");
        stored.ObjectId.Should().Be("oid-1");
        stored.ExternalUserId.Should().Be("sub-legacy");
        _factory.CountUsers().Should().Be(1);
    }

    [Fact]
    public async Task NewUser_IsCreatedByTidOid_WithNullExternalUserId()
    {
        var client = _factory.CreateAuthenticatedClient("sub-new", "Bea", "bea@demo.com", "tid-1", "oid-2");

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = _factory.AllUsers();
        users.Should().ContainSingle();
        users[0].TenantId.Should().Be("tid-1");
        users[0].ObjectId.Should().Be("oid-2");
        users[0].ExternalUserId.Should().BeNull();
    }

    [Fact]
    public async Task SecondLogin_SameUser_DoesNotCreateDuplicates()
    {
        var legacyId = _factory.SeedLegacyUser("sub-legacy", "Ana");
        var client = _factory.CreateAuthenticatedClient("sub-legacy", "Ana", null, "tid-1", "oid-1");

        await client.GetAsync("/api/v1/users/me");
        await client.GetAsync("/api/v1/users/me");

        _factory.CountUsers().Should().Be(1);
        _factory.AllUsers().Single().Id.Should().Be(legacyId);

        var newClient = _factory.CreateAuthenticatedClient("sub-x", "Bea", null, "tid-1", "oid-9");
        await newClient.GetAsync("/api/v1/users/me");
        await newClient.GetAsync("/api/v1/users/me");

        _factory.CountUsers().Should().Be(2);
    }

    [Fact]
    public async Task NewUser_ExternalUserId_IsNeverEmptyString()
    {
        var client = _factory.CreateAuthenticatedClient("sub-new", "Bea", null, "tid-1", "oid-2");

        await client.GetAsync("/api/v1/users/me");

        _factory.AllUsers().Single().ExternalUserId.Should().BeNull();
    }

    [Fact]
    public async Task LegacyProfile_ThatDoesNotAuthenticate_RemainsUntouched()
    {
        var untouchedId = _factory.SeedLegacyUser("sub-idle", "Idle");
        var client = _factory.CreateAuthenticatedClient("sub-other", "Otro", null, "tid-1", "oid-3");

        await client.GetAsync("/api/v1/users/me");

        var idle = _factory.FindUser(untouchedId);
        idle!.TenantId.Should().BeNull();
        idle.ObjectId.Should().BeNull();
        idle.ExternalUserId.Should().Be("sub-idle");
    }
}
