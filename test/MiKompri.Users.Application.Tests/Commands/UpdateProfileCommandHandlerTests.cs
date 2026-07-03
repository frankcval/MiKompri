using FluentAssertions;
using FluentValidation;
using MediatR;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Behavior;
using MiKompri.Users.Application.Commands.UpdateProfile;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Commands;

public class UpdateProfileCommandHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_ValidDisplayName_SavesAndReturnsDto()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        var user = new User("Ana", "ana@demo.com", "entra", "sub-1");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var handler = new UpdateProfileCommandHandler(_currentUser, _userRepository);

        var result = await handler.Handle(new UpdateProfileCommand("Ana María"), CancellationToken.None);

        result.DisplayName.Should().Be("Ana María");
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyDisplayName_ThrowsValidationException()
    {
        var handler = new UpdateProfileCommandHandler(_currentUser, _userRepository);
        var command = new UpdateProfileCommand(string.Empty);

        var act = () => ExecuteWithValidation(
            command,
            new UpdateProfileCommandValidator(),
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
