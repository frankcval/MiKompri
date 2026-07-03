using FluentAssertions;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Commands.AddMemberToGroup;
using MiKompri.Users.Domain.Abstractions;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Commands;

public class AddMemberToGroupCommandHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IGroupRepository _groupRepository = Substitute.For<IGroupRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_OwnerAddsAdmin_Succeeds()
    {
        var owner = Guid.NewGuid();
        var targetUser = new User("Target", "target@demo.com", "entra", "target-1");
        var group = new Group("Familia", owner);

        SetupCommon(owner, group, targetUser);

        var handler = BuildHandler();
        await handler.Handle(new AddMemberToGroupCommand(group.Id, targetUser.Id, GroupRole.Admin), CancellationToken.None);

        group.GetMemberRole(targetUser.Id).Should().Be(GroupRole.Admin);
    }

    [Fact]
    public async Task Handle_OwnerAddsMember_Succeeds()
    {
        var owner = Guid.NewGuid();
        var targetUser = new User("Target", "target@demo.com", "entra", "target-2");
        var group = new Group("Familia", owner);

        SetupCommon(owner, group, targetUser);

        var handler = BuildHandler();
        await handler.Handle(new AddMemberToGroupCommand(group.Id, targetUser.Id, GroupRole.Member), CancellationToken.None);

        group.GetMemberRole(targetUser.Id).Should().Be(GroupRole.Member);
    }

    [Fact]
    public async Task Handle_AdminAddsMember_Succeeds()
    {
        var owner = Guid.NewGuid();
        var admin = Guid.NewGuid();
        var targetUser = new User("Target", "target@demo.com", "entra", "target-3");
        var group = new Group("Familia", owner);
        group.AddMember(admin, GroupRole.Admin);

        SetupCommon(admin, group, targetUser);

        var handler = BuildHandler();
        await handler.Handle(new AddMemberToGroupCommand(group.Id, targetUser.Id, GroupRole.Member), CancellationToken.None);

        group.GetMemberRole(targetUser.Id).Should().Be(GroupRole.Member);
    }

    [Fact]
    public async Task Handle_AdminAddsAdmin_ThrowsForbiddenOperationException()
    {
        var owner = Guid.NewGuid();
        var admin = Guid.NewGuid();
        var targetUser = new User("Target", "target@demo.com", "entra", "target-4");
        var group = new Group("Familia", owner);
        group.AddMember(admin, GroupRole.Admin);

        SetupCommon(admin, group, targetUser);

        var handler = BuildHandler();
        var act = () => handler.Handle(new AddMemberToGroupCommand(group.Id, targetUser.Id, GroupRole.Admin), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task Handle_MemberTriesToAdd_ThrowsForbiddenOperationException()
    {
        var owner = Guid.NewGuid();
        var member = Guid.NewGuid();
        var targetUser = new User("Target", "target@demo.com", "entra", "target-5");
        var group = new Group("Familia", owner);
        group.AddMember(member, GroupRole.Member);

        SetupCommon(member, group, targetUser);

        var handler = BuildHandler();
        var act = () => handler.Handle(new AddMemberToGroupCommand(group.Id, targetUser.Id, GroupRole.Member), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task Handle_TargetUserNotFound_ThrowsInvalidOperationException()
    {
        var owner = Guid.NewGuid();
        var target = Guid.NewGuid();
        var group = new Group("Familia", owner);

        _currentUser.UserId.Returns(owner);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _userRepository.GetByIdAsync(target, Arg.Any<CancellationToken>()).Returns((User?)null);

        var handler = BuildHandler();
        var act = () => handler.Handle(new AddMemberToGroupCommand(group.Id, target, GroupRole.Member), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DuplicateMember_ThrowsInvalidOperationException()
    {
        var owner = Guid.NewGuid();
        var targetUser = new User("Target", "target@demo.com", "entra", "target-6");
        var group = new Group("Familia", owner);
        group.AddMember(targetUser.Id, GroupRole.Member);

        SetupCommon(owner, group, targetUser);

        var handler = BuildHandler();
        var act = () => handler.Handle(new AddMemberToGroupCommand(group.Id, targetUser.Id, GroupRole.Member), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private void SetupCommon(Guid callerId, Group group, User targetUser)
    {
        _currentUser.UserId.Returns(callerId);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _userRepository.GetByIdAsync(targetUser.Id, Arg.Any<CancellationToken>()).Returns(targetUser);
    }

    private AddMemberToGroupCommandHandler BuildHandler()
        => new(_currentUser, _groupRepository, _userRepository);
}
