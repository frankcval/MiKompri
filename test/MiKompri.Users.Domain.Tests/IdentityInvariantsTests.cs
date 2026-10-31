using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Domain.Tests;

public class IdentityInvariantsTests
{
    [Fact]
    public void LoginClaims_CannotOverwritePersonalization()
    {
        var user = new User("Imported name", null, "entra", "tenant", "object", "sub");
        user.UpdateProfile("My name");
        user.SyncClaims("Provider changed name", "provider@example.invalid");
        Assert.Equal("My name", user.DisplayName);
        Assert.Null(user.Email);
    }

    [Fact]
    public void AssignCanonicalIdentity_CannotSilentlyMoveAnExistingIdentity()
    {
        var user = new User("User", null, "entra", "tenant", "object", "sub");
        var id = user.Id;
        Assert.Throws<InvalidOperationException>(() => user.AssignCanonicalIdentity("other-tenant", "other-object"));
        Assert.Equal(id, user.Id);
        Assert.Equal("tenant", user.TenantId);
    }
}
