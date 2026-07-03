using FluentAssertions;
using MiKompri.Users.Domain.Abstractions;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Domain.Tests;

public class GroupTests
{
    [Fact]
    public void Constructor_RegistersOwnerMembership()
    {
        var ownerId = Guid.NewGuid();

        var group = new Group("Familia", ownerId);

        group.Memberships.Should().ContainSingle();
        var ownerMembership = group.Memberships.Single();
        ownerMembership.UserId.Should().Be(ownerId);
        ownerMembership.Role.Should().Be(GroupRole.Owner);
    }

    [Fact]
    public void AddMember_Duplicate_ThrowsInvalidOperationException()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var group = new Group("Familia", ownerId);
        group.AddMember(memberId, GroupRole.Member);

        var action = () => group.AddMember(memberId, GroupRole.Member);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveMember_LastOwner_ThrowsInvalidOperationException()
    {
        var ownerId = Guid.NewGuid();
        var group = new Group("Familia", ownerId);

        var action = () => group.RemoveMember(ownerId, GroupRole.Owner);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveMember_OwnerWithMultipleOwners_Succeeds()
    {
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();
        var group = new Group("Familia", owner1);
        group.AddMember(owner2, GroupRole.Owner);

        group.RemoveMember(owner1, GroupRole.Owner);

        group.Memberships.Should().NotContain(m => m.UserId == owner1);
        group.Memberships.Should().ContainSingle(m => m.UserId == owner2 && m.Role == GroupRole.Owner);
    }

    [Fact]
    public void RemoveMember_AdminRemovesMember_Succeeds()
    {
        var ownerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var group = new Group("Familia", ownerId);
        group.AddMember(adminId, GroupRole.Admin);
        group.AddMember(memberId, GroupRole.Member);

        group.RemoveMember(memberId, GroupRole.Admin);

        group.Memberships.Should().NotContain(m => m.UserId == memberId);
    }

    [Fact]
    public void RemoveMember_AdminRemovesAdmin_ThrowsForbiddenOperationException()
    {
        var ownerId = Guid.NewGuid();
        var admin1 = Guid.NewGuid();
        var admin2 = Guid.NewGuid();

        var group = new Group("Familia", ownerId);
        group.AddMember(admin1, GroupRole.Admin);
        group.AddMember(admin2, GroupRole.Admin);

        var action = () => group.RemoveMember(admin2, GroupRole.Admin);

        action.Should().Throw<ForbiddenOperationException>();
    }

    [Fact]
    public void RemoveMember_AdminRemovesOwner_ThrowsForbiddenOperationException()
    {
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var group = new Group("Familia", owner1);
        group.AddMember(owner2, GroupRole.Owner);
        group.AddMember(adminId, GroupRole.Admin);

        var action = () => group.RemoveMember(owner2, GroupRole.Admin);

        action.Should().Throw<ForbiddenOperationException>();
    }

    [Fact]
    public void RemoveMember_MemberNotInGroup_ThrowsKeyNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var nonMemberId = Guid.NewGuid();
        var group = new Group("Familia", ownerId);

        var action = () => group.RemoveMember(nonMemberId, GroupRole.Owner);

        action.Should().Throw<KeyNotFoundException>();
    }
}
