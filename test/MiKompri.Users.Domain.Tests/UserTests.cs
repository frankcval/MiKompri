using FluentAssertions;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_WithoutName_DisplayNameIsEmpty()
    {
        var user = new User(string.Empty, null, "entra", "sub-1");

        user.DisplayName.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_EmptyName_ThrowsInvalidOperationException()
    {
        var user = new User("Ana", "ana@demo.com", "entra", "sub-1");

        var action = () => user.UpdateProfile(" ");

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SyncClaims_WithDifferentValues_UpdatesAndSetsUpdatedAt()
    {
        var user = new User("Ana", "ana@demo.com", "entra", "sub-1");
        var before = user.UpdatedAt;

        Thread.Sleep(5);
        user.SyncClaims("Ana María", "anita@demo.com");

        user.DisplayName.Should().Be("Ana María");
        user.Email.Should().Be("anita@demo.com");
        user.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void SyncClaims_WithSameValues_DoesNotChangeUpdatedAt()
    {
        var user = new User("Ana", "ana@demo.com", "entra", "sub-1");
        var before = user.UpdatedAt;

        Thread.Sleep(5);
        user.SyncClaims("Ana", "ana@demo.com");

        user.UpdatedAt.Should().Be(before);
    }
}
