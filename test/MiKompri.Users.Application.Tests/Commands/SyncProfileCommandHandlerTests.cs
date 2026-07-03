using FluentAssertions;
using MiKompri.Users.Application.Commands.SyncProfile;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Commands;

public class SyncProfileCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_UserDoesNotExist_CreatesUserAndReturnsCreatedTrue()
    {
        _userRepository
            .GetByExternalIdAsync("entra", "sub-1", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new SyncProfileCommandHandler(_userRepository);
        var command = new SyncProfileCommand("entra", "sub-1", "Ana", "ana@demo.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Created.Should().BeTrue();
        result.UserId.Should().NotBe(Guid.Empty);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserExistsWithDifferentClaims_UpdatesAndReturnsCreatedFalse()
    {
        var existing = new User("Ana", "ana@demo.com", "entra", "sub-1");
        _userRepository
            .GetByExternalIdAsync("entra", "sub-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new SyncProfileCommandHandler(_userRepository);
        var command = new SyncProfileCommand("entra", "sub-1", "Ana Maria", "ana.maria@demo.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Created.Should().BeFalse();
        existing.DisplayName.Should().Be("Ana Maria");
        existing.Email.Should().Be("ana.maria@demo.com");
        await _userRepository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserExistsWithSameClaims_DoesNotSaveAndReturnsCreatedFalse()
    {
        var existing = new User("Ana", "ana@demo.com", "entra", "sub-1");
        _userRepository
            .GetByExternalIdAsync("entra", "sub-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new SyncProfileCommandHandler(_userRepository);
        var command = new SyncProfileCommand("entra", "sub-1", "Ana", "ana@demo.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Created.Should().BeFalse();
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
