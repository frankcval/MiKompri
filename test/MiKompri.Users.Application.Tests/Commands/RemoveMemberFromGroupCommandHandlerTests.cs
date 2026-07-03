using FluentAssertions;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Commands.RemoveMemberFromGroup;
using MiKompri.Users.Domain.Abstractions;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Commands;

public class RemoveMemberFromGroupCommandHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IGroupRepository _groupRepository = Substitute.For<IGroupRepository>();

    [Fact]
    public async Task Handle_OwnerRemovesMember_Succeeds()
    {
        var owner = Guid.NewGuid();
        var member = Guid.NewGuid();
        var group = new Group("Familia", owner);
        group.AddMember(member, GroupRole.Member);

        await ExecuteSuccess(owner, group, member);

        group.GetMemberRole(member).Should().BeNull();
    }

    [Fact]
    public async Task Handle_OwnerRemovesAdmin_Succeeds()
    {
        var owner = Guid.NewGuid();
        var admin = Guid.NewGuid();
        var group = new Group("Familia", owner);
        group.AddMember(admin, GroupRole.Admin);

        await ExecuteSuccess(owner, group, admin);

        group.GetMemberRole(admin).Should().BeNull();
    }

    [Fact]
    public async Task Handle_OwnerRemovesOwnerNotLast_Succeeds()
    {
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();
        var group = new Group("Familia", owner1);
        group.AddMember(owner2, GroupRole.Owner);

        await ExecuteSuccess(owner1, group, owner2);

        group.GetMemberRole(owner2).Should().BeNull();
    }

    [Fact]
    public async Task Handle_AdminRemovesMember_Succeeds()
    {
        var owner = Guid.NewGuid();
        var admin = Guid.NewGuid();
        var member = Guid.NewGuid();
        var group = new Group("Familia", owner);
        group.AddMember(admin, GroupRole.Admin);
        group.AddMember(member, GroupRole.Member);

        await ExecuteSuccess(admin, group, member);

        group.GetMemberRole(member).Should().BeNull();
    }

    [Fact]
    public async Task Handle_AdminRemovesAdmin_ThrowsForbiddenOperationException()
    {
        var owner = Guid.NewGuid();
        var admin1 = Guid.NewGuid();
        var admin2 = Guid.NewGuid();
        var group = new Group("Familia", owner);
        group.AddMember(admin1, GroupRole.Admin);
        group.AddMember(admin2, GroupRole.Admin);

        var act = () => Execute(admin1, group, admin2);

        await act.Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task Handle_AdminRemovesOwner_ThrowsForbiddenOperationException()
    {
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();
        var admin = Guid.NewGuid();
        var group = new Group("Familia", owner1);
        group.AddMember(owner2, GroupRole.Owner);
        group.AddMember(admin, GroupRole.Admin);

        var act = () => Execute(admin, group, owner2);

        await act.Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task Handle_LastOwnerRemovesSelf_ThrowsInvalidOperationException()
    {
        var owner = Guid.NewGuid();
        var group = new Group("Familia", owner);

        var act = () => Execute(owner, group, owner);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_GroupNotFound_ThrowsForbiddenOperationException()
    {
        var caller = Guid.NewGuid();
        var command = new RemoveMemberFromGroupCommand(Guid.NewGuid(), Guid.NewGuid());

        _currentUser.UserId.Returns(caller);
        _groupRepository.GetByIdAsync(command.GroupId, Arg.Any<CancellationToken>()).Returns((Group?)null);

        var handler = new RemoveMemberFromGroupCommandHandler(_currentUser, _groupRepository);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task Handle_TargetNotInGroup_ThrowsKeyNotFoundException()
    {
        var owner = Guid.NewGuid();
        var target = Guid.NewGuid();
        var group = new Group("Familia", owner);

        var act = () => Execute(owner, group, target);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private async Task ExecuteSuccess(Guid callerId, Group group, Guid targetUserId)
    {
        await Execute(callerId, group, targetUserId);
        await _groupRepository.Received(1).UpdateAsync(group, Arg.Any<CancellationToken>());
    }

    private async Task Execute(Guid callerId, Group group, Guid targetUserId)
    {
        _currentUser.UserId.Returns(callerId);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        var handler = new RemoveMemberFromGroupCommandHandler(_currentUser, _groupRepository);
        await handler.Handle(new RemoveMemberFromGroupCommand(group.Id, targetUserId), CancellationToken.None);
    }
}
