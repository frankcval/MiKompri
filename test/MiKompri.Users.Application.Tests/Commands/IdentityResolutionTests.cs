using MiKompri.Users.Application.Commands.SyncProfile;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Commands;
public class IdentityResolutionTests
{
    [Fact]
    public async Task ReservedMigrationCandidate_NeverProvisions()
    {
        var repo = Substitute.For<IUserRepository>();
        repo.IsIdentityReservedAsync("ciam", "target", "object", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new SyncProfileCommandHandler(repo);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new("ciam", "target", "object", "same-sub", "Name", "same@example.invalid"), default));
        await repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveOnly_DoesNotCreateUnknownUser()
    {
        var repo = Substitute.For<IUserRepository>();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new SyncProfileCommandHandler(repo).Handle(
            new("ciam", "target", "object", null, "Name", null, AllowProvision: false), default));
        await repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LegacySub_DefaultDoesNotAdoptAnotherCanonicalIdentity()
    {
        var repo = Substitute.For<IUserRepository>();
        var old = new User("Existing", null, "entra", "old", "old-object", "same-sub");
        repo.GetByExternalIdAsync("entra", "same-sub", Arg.Any<CancellationToken>()).Returns(old);
        await new SyncProfileCommandHandler(repo).Handle(new("entra", "new", "new-object", "same-sub", "New", null), default);
        await repo.DidNotReceive().GetByExternalIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.Equal("old", old.TenantId);
    }
}
