using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MiKompri.Users.Api.Models;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.Users.Api.Tests;

public class ProfileApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProfileApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task GetMe_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithAuth_Returns200AndAutoProvisionedProfile()
    {
        var client = _factory.CreateAuthenticatedClient("sub-1", "Ana", "ana@demo.com");

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        profile.Should().NotBeNull();
        profile!.DisplayName.Should().Be("Ana");
        profile.Email.Should().Be("ana@demo.com");
    }

    [Fact]
    public async Task GetMe_SecondCallWithSameToken_ReturnsSameIdAndNoDuplicates()
    {
        var client = _factory.CreateAuthenticatedClient("sub-2", "Luis", "luis@demo.com");

        var first = await client.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");
        var second = await client.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");

        first.Should().NotBeNull();
        second.Should().NotBeNull();
        second!.Id.Should().Be(first!.Id);
        _factory.CountUsers().Should().Be(1);
    }

    [Fact]
    public async Task PutMe_WithValidDisplayName_Returns200()
    {
        var client = _factory.CreateAuthenticatedClient("sub-3", "User", "user@demo.com");
        await client.GetAsync("/api/v1/users/me");

        var response = await client.PutAsJsonAsync("/api/v1/users/me", new UpdateProfileRequest
        {
            DisplayName = "Nombre actualizado"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        dto!.DisplayName.Should().Be("Nombre actualizado");
    }

    [Fact]
    public async Task PutMe_WithEmptyDisplayName_Returns400WithFieldDisplayName()
    {
        var client = _factory.CreateAuthenticatedClient("sub-4", "User", "user@demo.com");
        await client.GetAsync("/api/v1/users/me");

        var response = await client.PutAsJsonAsync("/api/v1/users/me", new UpdateProfileRequest
        {
            DisplayName = string.Empty
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("errors")[0].GetProperty("field").GetString().Should().Be("DisplayName");
    }

    [Fact]
    public async Task PostSync_FirstCall_Returns201()
    {
        var client = _factory.CreateAuthenticatedClient("sub-5", "Carla", "carla@demo.com");

        var response = await client.PostAsync("/api/v1/users/me/sync", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostSync_SecondCallWithSameToken_Returns200()
    {
        var client = _factory.CreateAuthenticatedClient("sub-6", "Pablo", "pablo@demo.com");

        var first = await client.PostAsync("/api/v1/users/me/sync", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
        var second = await client.PostAsync("/api/v1/users/me/sync", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostSync_WithTokenWithoutSub_Returns401WithGenericBody()
    {
        var client = _factory.CreateAuthenticatedClient(sub: string.Empty, name: "NoSub", email: "nosub@demo.com");

        var response = await client.PostAsync("/api/v1/users/me/sync", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("status").GetInt32().Should().Be(401);
        body.RootElement.GetProperty("error").GetString().Should().Be("Authentication failed.");
        body.RootElement.TryGetProperty("traceId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SwaggerJson_WithoutAuth_Returns200AndApplicationJson()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Contain("application/json");
    }
}
