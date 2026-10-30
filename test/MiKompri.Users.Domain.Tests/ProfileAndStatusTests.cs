using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Domain.Tests;
public class ProfileAndStatusTests
{
    [Fact]
    public void Profile_ClearsStayClearedAndVersionIsRequired()
    {
        var user = new User("Imported", null, "ciam", "tenant", "object", null);
        user.UpdateDetails(new ProfileDetails("Frank", "frank_cv", null, null, "es", "newest"), user.ProfileVersion);
        user.SyncClaims("External", "changed@example.invalid");
        Assert.Equal("Frank", user.DisplayName);
        Assert.Null(user.GivenName);
        Assert.Equal("frank_cv", user.Username);
        Assert.Throws<ProfileConflictException>(() => user.UpdateDetails(new ProfileDetails("Stale", null, null, null, null, "newest"), 0));
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("ab")]
    [InlineData("person@example.invalid")]
    public void Username_RejectsReservedOrInvalidValues(string username)
    {
        var user = new User("User", null, "ciam", "tenant", "object", null);
        Assert.Throws<InvalidOperationException>(() => user.UpdateDetails(new("User", username, null, null, "es", "newest"), user.ProfileVersion));
    }

    [Fact]
    public void SuspensionAndReactivation_DoNotResurrectOldTokensIncludingSameSecond()
    {
        var user = new User("User", null, "ciam", "tenant", "object", null);
        var now = DateTimeOffset.FromUnixTimeSeconds(1000);
        user.SetSuspended(true, now);
        Assert.True(user.IsSuspended);
        user.SetSuspended(false, now.AddSeconds(10));
        Assert.False(user.IsSuspended);
        Assert.False(user.AcceptsIssuedAt(1009));
        Assert.False(user.AcceptsIssuedAt(1010));
        Assert.False(user.AcceptsIssuedAt(null));
        Assert.True(user.AcceptsIssuedAt(1011));
    }
}
