using FluentAssertions;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Domain.Tests;

public class UserIdentityTests
{
    [Fact]
    public void CreateFromCanonicalIdentity_SetsTenantAndObjectId_AndNullExternalUserId()
    {
        var user = User.CreateFromCanonicalIdentity("Ana", "ana@demo.com", "entra", "tid-1", "oid-1");

        user.Id.Should().NotBe(Guid.Empty);
        user.TenantId.Should().Be("tid-1");
        user.ObjectId.Should().Be("oid-1");
        user.ExternalUserId.Should().BeNull();
    }

    [Fact]
    public void LegacyConstructor_LeavesTenantAndObjectIdNull()
    {
        var user = new User("Ana", "ana@demo.com", "entra", "sub-1");

        user.ExternalUserId.Should().Be("sub-1");
        user.TenantId.Should().BeNull();
        user.ObjectId.Should().BeNull();
    }

    [Fact]
    public void AssociateCanonicalIdentity_OnLegacyUser_KeepsIdAndSetsTidOid()
    {
        var user = new User("Ana", null, "entra", "sub-1");
        var originalId = user.Id;

        var changed = user.AssociateCanonicalIdentity("tid-1", "oid-1");

        changed.Should().BeTrue();
        user.Id.Should().Be(originalId);
        user.TenantId.Should().Be("tid-1");
        user.ObjectId.Should().Be("oid-1");
        user.ExternalUserId.Should().Be("sub-1");
    }

    [Fact]
    public void AssociateCanonicalIdentity_WhenAlreadyAssociated_ReturnsFalse()
    {
        var user = User.CreateFromCanonicalIdentity("Ana", null, "entra", "tid-1", "oid-1");

        user.AssociateCanonicalIdentity("tid-1", "oid-1").Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LegacyConstructor_EmptyExternalUserId_Throws(string externalUserId)
    {
        var action = () => new User("Ana", null, "entra", externalUserId);

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("", "oid-1")]
    [InlineData("tid-1", "")]
    [InlineData(" ", " ")]
    public void CreateFromCanonicalIdentity_EmptyTidOrOid_Throws(string tid, string oid)
    {
        var action = () => User.CreateFromCanonicalIdentity("Ana", null, "entra", tid, oid);

        action.Should().Throw<ArgumentException>();
    }
}
