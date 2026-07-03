using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Queries.GetGroupMembers;
using MiKompri.Users.Domain.Abstractions;
using MiKompri.Users.Domain.Users;
using MiKompri.Users.Infrastructure.Persistence;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Queries;

public class GetGroupMembersQueryHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IGroupRepository _groupRepository = Substitute.For<IGroupRepository>();

    [Fact]
    public async Task Handle_CallerIsMember_ReturnsDisplayNameAndJoinedAt()
    {
        var ownerUser = new User("Owner", "owner@demo.com", "entra", "owner-sub");
        var memberUser = new User("Member", "member@demo.com", "entra", "member-sub");

        var group = new Group("Familia", ownerUser.Id);
        group.AddMember(memberUser.Id, GroupRole.Member);

        _currentUser.UserId.Returns(ownerUser.Id);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        using var context = CreateContext();
        context.Users.AddRange(ownerUser, memberUser);
        await context.SaveChangesAsync();

        var handler = new GetGroupMembersQueryHandler(_currentUser, _groupRepository, context);

        var result = await handler.Handle(new GetGroupMembersQuery(group.Id), CancellationToken.None);

        result.Should().Contain(x => x.UserId == ownerUser.Id && x.DisplayName == "Owner");
        result.Should().Contain(x => x.UserId == memberUser.Id && x.JoinedAt != default);
    }

    [Fact]
    public async Task Handle_CallerIsNotMember_ThrowsForbiddenOperationException()
    {
        var ownerUser = new User("Owner", "owner@demo.com", "entra", "owner-sub");
        var outsider = Guid.NewGuid();

        var group = new Group("Familia", ownerUser.Id);

        _currentUser.UserId.Returns(outsider);
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        using var context = CreateContext();
        context.Users.Add(ownerUser);
        await context.SaveChangesAsync();

        var handler = new GetGroupMembersQueryHandler(_currentUser, _groupRepository, context);

        var act = () => handler.Handle(new GetGroupMembersQuery(group.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenOperationException>();
    }

    private static UsersDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase($"users-app-tests-{Guid.NewGuid()}")
            .Options;

        return new UsersDbContext(options);
    }
}
