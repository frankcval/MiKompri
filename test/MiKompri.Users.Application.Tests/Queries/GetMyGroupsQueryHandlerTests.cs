using FluentAssertions;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Queries.GetMyGroups;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Queries;

public class GetMyGroupsQueryHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IGroupRepository _groupRepository = Substitute.For<IGroupRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_CallerWithGroups_ReturnsListWithMyRole()
    {
        var caller = Guid.NewGuid();
        var other = Guid.NewGuid();

        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(caller);

        var group = new Group("Familia", caller);
        group.AddMember(other, GroupRole.Member);

        _groupRepository.GetByUserIdAsync(caller, Arg.Any<CancellationToken>())
            .Returns(new[] { group });

        _userRepository.GetByIdAsync(caller, Arg.Any<CancellationToken>())
            .Returns(new User("Owner", "owner@demo.com", "entra", "owner-sub"));
        _userRepository.GetByIdAsync(other, Arg.Any<CancellationToken>())
            .Returns(new User("Member", "member@demo.com", "entra", "member-sub"));

        var handler = new GetMyGroupsQueryHandler(_currentUser, _groupRepository, _userRepository);

        var result = await handler.Handle(new GetMyGroupsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result.Single().MyRole.Should().Be("Owner");
    }

    [Fact]
    public async Task Handle_CallerWithoutGroups_ReturnsEmptyCollection()
    {
        var caller = Guid.NewGuid();

        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(caller);

        _groupRepository.GetByUserIdAsync(caller, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Group>());

        var handler = new GetMyGroupsQueryHandler(_currentUser, _groupRepository, _userRepository);

        var result = await handler.Handle(new GetMyGroupsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
