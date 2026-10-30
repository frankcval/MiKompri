using MiKompri.Users.Domain.Users;
namespace MiKompri.Users.Domain.Tests;
public class MicrosoftMigrationTests
{
    private static User Source() => new("Owned", null, "entra", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "legacy-sub");
    [Fact]
    public void CutoverAndRollback_PreserveIdProfileAndSuspension()
    {
        var user = Source(); var id = user.Id; var tid = user.TenantId;
        var migration = new IdentityMigration(user, "ciam", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new('a', 64), new('b', 64), DateTimeOffset.UtcNow);
        migration.Apply(user, 0, DateTimeOffset.FromUnixTimeSeconds(1000));
        Assert.Equal(id, user.Id); Assert.Equal("Owned", user.DisplayName); Assert.Equal("ciam", user.IdentityProvider);
        Assert.Null(user.ExternalUserId); Assert.False(user.AcceptsIssuedAt(1000));
        user.SetSuspended(true, DateTimeOffset.FromUnixTimeSeconds(1001));
        migration.Rollback(user, 1, DateTimeOffset.FromUnixTimeSeconds(1002));
        Assert.Equal(id, user.Id); Assert.Equal(tid, user.TenantId); Assert.True(user.IsSuspended);
    }
    [Fact]
    public void WrongUserOrStaleVersion_DeniesWithoutChangingIdentity()
    {
        var user = Source(); var other = Source(); var tid = other.TenantId;
        var migration = new IdentityMigration(user, "ciam", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new('a', 64), new('b', 64), DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => migration.Apply(other, 0, DateTimeOffset.UtcNow));
        Assert.Equal(tid, other.TenantId);
        Assert.Throws<InvalidOperationException>(() => migration.Apply(user, 1, DateTimeOffset.UtcNow));
    }
}
