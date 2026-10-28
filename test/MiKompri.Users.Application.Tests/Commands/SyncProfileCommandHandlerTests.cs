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
            .GetByCanonicalIdentityAsync("entra", "tenant-1", "object-1", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _userRepository
            .GetByExternalIdAsync("entra", "legacy-sub-1", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new SyncProfileCommandHandler(_userRepository);
        var command = new SyncProfileCommand("entra", "tenant-1", "object-1", "legacy-sub-1", "Ana", "ana@demo.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Created.Should().BeTrue();
        result.UserId.Should().NotBe(Guid.Empty);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserExistsByCanonicalIdentity_UpdatesAndReturnsCreatedFalse()
    {
        var existing = new User("Ana", "ana@demo.com", "entra", "tenant-1", "object-1", "legacy-sub-1");
        _userRepository
            .GetByCanonicalIdentityAsync("entra", "tenant-1", "object-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new SyncProfileCommandHandler(_userRepository);
        var command = new SyncProfileCommand("entra", "tenant-1", "object-1", "legacy-sub-1", "Ana Maria", "ana.maria@demo.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Created.Should().BeFalse();
        existing.DisplayName.Should().Be("Ana Maria");
        existing.Email.Should().Be("ana.maria@demo.com");
        await _userRepository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserFoundByLegacySub_AssignsCanonicalIdentity()
    {
        var existing = new User("Ana", "ana@demo.com", "entra", null, null, "legacy-sub-1");

        _userRepository
            .GetByCanonicalIdentityAsync("entra", "tenant-1", "object-1", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _userRepository
            .GetByExternalIdAsync("entra", "legacy-sub-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new SyncProfileCommandHandler(_userRepository);
        var command = new SyncProfileCommand("entra", "tenant-1", "object-1", "legacy-sub-1", "Ana", "ana@demo.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Created.Should().BeFalse();
        existing.TenantId.Should().Be("tenant-1");
        existing.ObjectId.Should().Be("object-1");
        await _userRepository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }
}
