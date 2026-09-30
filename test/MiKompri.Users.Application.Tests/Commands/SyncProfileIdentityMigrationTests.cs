using FluentAssertions;
using MiKompri.Users.Application.Commands.SyncProfile;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Commands;

public class SyncProfileIdentityMigrationTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();

    private SyncProfileCommandHandler CreateHandler() => new(_repo);

    [Fact]
    public async Task Handle_NewUserWithTidOid_CreatesCanonicalUserWithNullExternalUserId()
    {
        _repo.GetByCanonicalIdentityAsync("tid-1", "oid-1", Arg.Any<CancellationToken>()).Returns((User?)null);
        _repo.GetByExternalIdAsync("entra", "sub-1", Arg.Any<CancellationToken>()).Returns((User?)null);
        User? added = null;
        await _repo.AddAsync(Arg.Do<User>(u => added = u), Arg.Any<CancellationToken>());

        var result = await CreateHandler().Handle(
            new SyncProfileCommand("entra", "sub-1", "Ana", "ana@demo.com", "tid-1", "oid-1"),
            CancellationToken.None);

        result.Created.Should().BeTrue();
        added.Should().NotBeNull();
        added!.TenantId.Should().Be("tid-1");
        added.ObjectId.Should().Be("oid-1");
        added.ExternalUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_LegacyUserAuthenticates_KeepsUserIdAndAssociatesTidOid()
    {
        var legacy = new User("Ana", "ana@demo.com", "entra", "sub-1");
        var originalId = legacy.Id;
        _repo.GetByCanonicalIdentityAsync("tid-1", "oid-1", Arg.Any<CancellationToken>()).Returns((User?)null);
        _repo.GetByExternalIdAsync("entra", "sub-1", Arg.Any<CancellationToken>()).Returns(legacy);

        var result = await CreateHandler().Handle(
            new SyncProfileCommand("entra", "sub-1", "Ana", "ana@demo.com", "tid-1", "oid-1"),
            CancellationToken.None);

        result.Created.Should().BeFalse();
        result.UserId.Should().Be(originalId);
        legacy.TenantId.Should().Be("tid-1");
        legacy.ObjectId.Should().Be("oid-1");
        await _repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).UpdateAsync(legacy, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyMigratedUser_ResolvesByTidOidWithoutDuplicates()
    {
        var migrated = User.CreateFromCanonicalIdentity("Ana", "ana@demo.com", "entra", "tid-1", "oid-1");
        _repo.GetByCanonicalIdentityAsync("tid-1", "oid-1", Arg.Any<CancellationToken>()).Returns(migrated);

        var result = await CreateHandler().Handle(
            new SyncProfileCommand("entra", "sub-2", "Ana", "ana@demo.com", "tid-1", "oid-1"),
            CancellationToken.None);

        result.Created.Should().BeFalse();
        result.UserId.Should().Be(migrated.Id);
        await _repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().GetByExternalIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NeverCreatesDuplicate_WhenLegacyExists()
    {
        var legacy = new User("Ana", null, "entra", "sub-1");
        _repo.GetByCanonicalIdentityAsync("tid-1", "oid-1", Arg.Any<CancellationToken>()).Returns((User?)null);
        _repo.GetByExternalIdAsync("entra", "sub-1", Arg.Any<CancellationToken>()).Returns(legacy);

        await CreateHandler().Handle(
            new SyncProfileCommand("entra", "sub-1", "Ana", null, "tid-1", "oid-1"),
            CancellationToken.None);

        await _repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LegacyUserWhoNeverAuthenticatesWithTidOid_RemainsUntouched()
    {
        var legacy = new User("Ana", "ana@demo.com", "entra", "sub-1");
        _repo.GetByExternalIdAsync("entra", "sub-1", Arg.Any<CancellationToken>()).Returns(legacy);

        await CreateHandler().Handle(
            new SyncProfileCommand("entra", "sub-1", "Ana", "ana@demo.com"),
            CancellationToken.None);

        legacy.TenantId.Should().BeNull();
        legacy.ObjectId.Should().BeNull();
        await _repo.DidNotReceive().GetByCanonicalIdentityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
