using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MiKompri.Users.Api.Models;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.Users.Api.Tests;

public class GroupsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GroupsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task PostGroups_ValidName_Returns201AndLocation()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-1", "Owner", "owner@demo.com");

        var response = await owner.PostAsJsonAsync("/api/v1/groups", new CreateGroupRequest { Name = "Familia" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostGroups_EmptyName_Returns400()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-2", "Owner", "owner@demo.com");

        var response = await owner.PostAsJsonAsync("/api/v1/groups", new CreateGroupRequest { Name = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetGroups_WithGroups_Returns200WithMyRole()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-3", "Owner", "owner@demo.com");
        await owner.PostAsJsonAsync("/api/v1/groups", new CreateGroupRequest { Name = "Familia" });

        var response = await owner.GetAsync("/api/v1/groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<GroupDto>>();
        groups.Should().NotBeNullOrEmpty();
        groups!.First().MyRole.Should().Be("Owner");
    }

    [Fact]
    public async Task GetGroups_WithoutGroups_Returns200EmptyArray()
    {
        var user = _factory.CreateAuthenticatedClient("user-4", "User", "user@demo.com");

        var response = await user.GetAsync("/api/v1/groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<GroupDto>>();
        groups.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMembers_AsOwner_Returns200WithMembers()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-5", "Owner", "owner@demo.com");
        var group = await CreateGroup(owner, "Casa");

        var response = await owner.GetAsync($"/api/v1/groups/{group.Id}/members");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await response.Content.ReadFromJsonAsync<List<GroupMemberDto>>();
        members.Should().Contain(m => m.Role == "Owner");
    }

    [Fact]
    public async Task GetMembers_WithoutMembership_Returns403()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-6", "Owner", "owner@demo.com");
        var outsider = _factory.CreateAuthenticatedClient("outsider-6", "Out", "out@demo.com");
        var group = await CreateGroup(owner, "Casa");

        var response = await outsider.GetAsync($"/api/v1/groups/{group.Id}/members");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OwnerAddsMember_Returns201()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-7", "Owner", "owner@demo.com");
        var member = _factory.CreateAuthenticatedClient("member-7", "Member", "member@demo.com");

        await member.GetAsync("/api/v1/users/me");
        var group = await CreateGroup(owner, "Casa");

        var memberProfile = await member.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");

        var response = await owner.PostAsJsonAsync($"/api/v1/groups/{group.Id}/members", new AddGroupMemberRequest
        {
            UserId = memberProfile!.Id,
            Role = "Member"
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, $"Body: {body}");
    }

    [Fact]
    public async Task AdminTryingToAddAdmin_Returns403()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-8", "Owner", "owner@demo.com");
        var admin = _factory.CreateAuthenticatedClient("admin-8", "Admin", "admin@demo.com");
        var target = _factory.CreateAuthenticatedClient("target-8", "Target", "target@demo.com");

        var group = await CreateGroup(owner, "Casa");

        var adminProfile = await admin.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");
        var targetProfile = await target.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");

        await owner.PostAsJsonAsync($"/api/v1/groups/{group.Id}/members", new AddGroupMemberRequest
        {
            UserId = adminProfile!.Id,
            Role = "Admin"
        });

        var response = await admin.PostAsJsonAsync($"/api/v1/groups/{group.Id}/members", new AddGroupMemberRequest
        {
            UserId = targetProfile!.Id,
            Role = "Admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminRemovesAdmin_Returns403()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-9", "Owner", "owner@demo.com");
        var admin1 = _factory.CreateAuthenticatedClient("admin1-9", "Admin1", "admin1@demo.com");
        var admin2 = _factory.CreateAuthenticatedClient("admin2-9", "Admin2", "admin2@demo.com");

        var group = await CreateGroup(owner, "Casa");

        var admin1Profile = await admin1.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");
        var admin2Profile = await admin2.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");

        await owner.PostAsJsonAsync($"/api/v1/groups/{group.Id}/members", new AddGroupMemberRequest { UserId = admin1Profile!.Id, Role = "Admin" });
        await owner.PostAsJsonAsync($"/api/v1/groups/{group.Id}/members", new AddGroupMemberRequest { UserId = admin2Profile!.Id, Role = "Admin" });

        var response = await admin1.DeleteAsync($"/api/v1/groups/{group.Id}/members/{admin2Profile.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DuplicateMember_Returns400()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-10", "Owner", "owner@demo.com");
        var member = _factory.CreateAuthenticatedClient("member-10", "Member", "member@demo.com");

        var group = await CreateGroup(owner, "Casa");
        var memberProfile = await member.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");

        await owner.PostAsJsonAsync($"/api/v1/groups/{group.Id}/members", new AddGroupMemberRequest
        {
            UserId = memberProfile!.Id,
            Role = "Member"
        });

        var duplicate = await owner.PostAsJsonAsync($"/api/v1/groups/{group.Id}/members", new AddGroupMemberRequest
        {
            UserId = memberProfile.Id,
            Role = "Member"
        });

        var body = await duplicate.Content.ReadAsStringAsync();
        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"Body: {body}");
    }

    [Fact]
    public async Task RemoveSingleOwner_Returns400()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-11", "Owner", "owner@demo.com");
        var group = await CreateGroup(owner, "Casa");
        var ownerProfile = await owner.GetFromJsonAsync<UserProfileDto>("/api/v1/users/me");

        var response = await owner.DeleteAsync($"/api/v1/groups/{group.Id}/members/{ownerProfile!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMembers_NonExistingGroup_Returns403()
    {
        var owner = _factory.CreateAuthenticatedClient("owner-12", "Owner", "owner@demo.com");

        var response = await owner.GetAsync($"/api/v1/groups/{Guid.NewGuid()}/members");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task<GroupDto> CreateGroup(HttpClient ownerClient, string name)
    {
        var response = await ownerClient.PostAsJsonAsync("/api/v1/groups", new CreateGroupRequest { Name = name });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        group.Should().NotBeNull();
        return group!;
    }
}
