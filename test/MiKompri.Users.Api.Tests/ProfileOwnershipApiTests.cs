using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MiKompri.Users.Api.Tests;
public class ProfileOwnershipApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public ProfileOwnershipApiTests(CustomWebApplicationFactory factory) { _factory = factory; factory.ResetDatabase(); }
    [Theory]
    [InlineData("userId")]
    [InlineData("tenantId")]
    [InlineData("objectId")]
    [InlineData("identityProvider")]
    [InlineData("isSuspended")]
    [InlineData("securityVersion")]
    [InlineData("emailVerified")]
    public async Task ProfileCannotAssignIdentityOrSecurityFields(string field)
    {
        using var client = _factory.CreateAuthenticatedClient("profile-field-guard", "Owned");
        var before = await client.GetFromJsonAsync<JsonElement>("/api/v1/users/me");
        client.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "\"0\"");
        var response = await client.PutAsJsonAsync("/api/v1/users/me", new Dictionary<string, object?> {
            ["displayName"] = "Do not save", [field] = "untrusted-client-value"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var after = await client.GetFromJsonAsync<JsonElement>("/api/v1/users/me");
        Assert.Equal(before.GetProperty("id").GetGuid(), after.GetProperty("id").GetGuid());
        Assert.Equal("Owned", after.GetProperty("displayName").GetString());
    }
    [Fact]
    public async Task ExtendedProfile_RequiresVersionAndKeepsLegacyNameOnlyCompatible()
    {
        using var client = _factory.CreateAuthenticatedClient("profile-version", "Imported");
        await client.GetAsync("/api/v1/users/me");
        var response = await client.PutAsJsonAsync("/api/v1/users/me", new { displayName = "Mine", username = "mine_user" });
        Assert.Equal((HttpStatusCode)428, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync("/api/v1/users/me", new { displayName = "Mine" })).StatusCode);
    }
    [Fact]
    public async Task ProfileFields_AreOwnedAndClearedWithOptimisticConcurrency()
    {
        using var client = _factory.CreateAuthenticatedClient("profile-details", "Imported");
        var initial = await client.GetFromJsonAsync<JsonElement>("/api/v1/users/me");
        var version = initial.GetProperty("profileVersion").GetInt64();
        client.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        var result = await client.PutAsJsonAsync("/api/v1/users/me", new { displayName = "My name", username = "my_name", givenName = "Given", familyName = (string?)null, preferredLanguage = "es", defaultListSort = "oldest" });
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await client.PutAsJsonAsync("/api/v1/users/me", new { displayName = "Stale", username = "other_name" })).StatusCode);
        var updated = await result.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Remove("If-Match");
        client.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", $"\"{updated.GetProperty("profileVersion").GetInt64()}\"");
        Assert.True((await client.PutAsJsonAsync("/api/v1/users/me", new { displayName = "My name", givenName = (string?)null })).IsSuccessStatusCode);
        await client.PostAsync("/api/v1/users/me/sync", null);
        var profile = await client.GetFromJsonAsync<JsonElement>("/api/v1/users/me");
        Assert.Equal("My name", profile.GetProperty("displayName").GetString());
        Assert.Equal("my_name", profile.GetProperty("username").GetString());
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("givenName").ValueKind);
        Assert.Equal("oldest", profile.GetProperty("defaultListSort").GetString());
    }
}
