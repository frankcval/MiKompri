using FluentAssertions;
using FluentValidation;
using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Behavior;
using MiKompri.Users.Application.Commands.CreateGroup;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Commands;

public class CreateGroupCommandHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IGroupRepository _groupRepository = Substitute.For<IGroupRepository>();

    [Fact]
    public async Task Handle_NotAuthenticated_ThrowsInvalidOperationException()
    {
        _currentUser.IsAuthenticated.Returns(false);
        var handler = new CreateGroupCommandHandler(_currentUser, _groupRepository);

        var act = () => handler.Handle(new CreateGroupCommand("Familia"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ValidName_ReturnsGroupId()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new CreateGroupCommandHandler(_currentUser, _groupRepository);

        var id = await handler.Handle(new CreateGroupCommand("Familia"), CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        await _groupRepository.Received(1).AddAsync(Arg.Any<Group>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnerRegisteredAutomaticallyInMemberships()
    {
        var ownerId = Guid.NewGuid();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(ownerId);

        Group? capturedGroup = null;
        _groupRepository
            .When(r => r.AddAsync(Arg.Any<Group>(), Arg.Any<CancellationToken>()))
            .Do(call => capturedGroup = call.Arg<Group>());

        var handler = new CreateGroupCommandHandler(_currentUser, _groupRepository);

        await handler.Handle(new CreateGroupCommand("Familia"), CancellationToken.None);

        capturedGroup.Should().NotBeNull();
        capturedGroup!.Memberships.Should().ContainSingle(m => m.UserId == ownerId && m.Role == GroupRole.Owner);
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsValidationException()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(Guid.NewGuid());

        var handler = new CreateGroupCommandHandler(_currentUser, _groupRepository);
        var command = new CreateGroupCommand(string.Empty);

        var act = () => ExecuteWithValidation(
            command,
            new CreateGroupCommandValidator(),
            () => handler.Handle(command, CancellationToken.None));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_NameWith101Chars_ThrowsValidationException()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(Guid.NewGuid());

        var handler = new CreateGroupCommandHandler(_currentUser, _groupRepository);
        var command = new CreateGroupCommand(new string('a', 101));

        var act = () => ExecuteWithValidation(
            command,
            new CreateGroupCommandValidator(),
            () => handler.Handle(command, CancellationToken.None));

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static async Task<TResponse> ExecuteWithValidation<TRequest, TResponse>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<Task<TResponse>> next)
        where TRequest : notnull
    {
        var behavior = new ValidationBehavior<TRequest, TResponse>(new[] { validator });
        return await behavior.Handle(request, () => next(), CancellationToken.None);
    }
}
