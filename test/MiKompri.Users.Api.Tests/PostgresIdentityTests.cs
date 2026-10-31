using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Application.Commands.SyncProfile;
using MiKompri.Users.Domain.Users;
using MiKompri.Users.Infrastructure.Identity;
using MiKompri.Users.Infrastructure.Persistence;
using MiKompri.Users.Infrastructure.Persistence.Repositories;
using NSubstitute;
using Npgsql;

namespace MiKompri.Users.Api.Tests;
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute() { if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MIKOMPRI_TEST_POSTGRES"))) Skip = "Set MIKOMPRI_TEST_POSTGRES to an isolated Docker PostgreSQL."; }
}

public class PostgresIdentityTests
{
    [PostgresFact]
    public async Task ReservationRetry_ReturnsSameJournal_AndRejectsChangedTargetOrApproval()
    {
        await using var database = await TestDatabase.CreateAsync();
        var source = new User("Owned", null, "entra", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "legacy");
        await using (var seed = database.Open()) { seed.Users.Add(source); await seed.SaveChangesAsync(); }
        var targetTid = Guid.NewGuid().ToString(); var targetOid = Guid.NewGuid().ToString();
        var verifier = Substitute.For<IFederatedOriginVerifier>();
        verifier.VerifyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new string('b', 64));
        Guid journalId;
        await using (var db = database.Open()) journalId = (await new MicrosoftIdentityMigrationService(db, verifier, TimeProvider.System)
            .ReserveAsync(source.Id, targetTid, targetOid, new string('a', 64))).Id;
        await using (var retry = database.Open()) Assert.Equal(journalId, (await new MicrosoftIdentityMigrationService(retry, verifier, TimeProvider.System)
            .ReserveAsync(source.Id, targetTid, targetOid, new string('a', 64))).Id);
        await using (var conflict = database.Open()) await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MicrosoftIdentityMigrationService(conflict, verifier, TimeProvider.System).ReserveAsync(source.Id, targetTid, Guid.NewGuid().ToString(), new string('a', 64)));
        await using (var conflict = database.Open()) await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MicrosoftIdentityMigrationService(conflict, verifier, TimeProvider.System).ReserveAsync(source.Id, targetTid, targetOid, new string('c', 64)));
        await using var verify = database.Open();
        Assert.Equal("Reserved", (await verify.IdentityMigrations.SingleAsync()).State);
        Assert.Equal(source.ObjectId, (await verify.Users.SingleAsync()).ObjectId);
    }

    [PostgresFact]
    public async Task OriginVerificationFailureBeforeCutover_PreservesLegacyAndAllowsSafeRetry()
    {
        await using var database = await TestDatabase.CreateAsync();
        var source = new User("Owned", null, "entra", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "legacy");
        await using (var seed = database.Open()) { seed.Users.Add(source); await seed.SaveChangesAsync(); }
        var verifier = Substitute.For<IFederatedOriginVerifier>();
        verifier.VerifyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new string('b', 64));
        Guid journalId;
        await using (var reserve = database.Open()) journalId = (await new MicrosoftIdentityMigrationService(reserve, verifier, TimeProvider.System)
            .ReserveAsync(source.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new string('a', 64))).Id;
        verifier.VerifyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(new HttpRequestException("QA Graph unavailable")));
        await using (var attempt = database.Open()) await Assert.ThrowsAsync<HttpRequestException>(() =>
            new MicrosoftIdentityMigrationService(attempt, verifier, TimeProvider.System).ChangeAsync(journalId, 0, false));
        await using (var unchanged = database.Open())
        {
            Assert.Equal("Reserved", (await unchanged.IdentityMigrations.SingleAsync()).State);
            Assert.Equal("entra", (await unchanged.Users.SingleAsync()).IdentityProvider);
            Assert.Equal(source.ObjectId, (await unchanged.Users.SingleAsync()).ObjectId);
        }
        verifier.VerifyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new string('b', 64));
        await using (var retry = database.Open()) await new MicrosoftIdentityMigrationService(retry, verifier, TimeProvider.System).ChangeAsync(journalId, 0, false);
        await using var verify = database.Open();
        Assert.Equal(source.Id, (await verify.Users.SingleAsync()).Id);
        Assert.Equal("Applied", (await verify.IdentityMigrations.SingleAsync()).State);
    }

    [PostgresFact]
    public async Task Rollback_PreservesNewProfileGroupsAndSuspension_AndIsIdempotent()
    {
        await using var database = await TestDatabase.CreateAsync();
        var source = new User("Before", null, "entra", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "legacy");
        await using (var seed = database.Open()) { seed.Users.Add(source); await seed.SaveChangesAsync(); }
        var verifier = Substitute.For<IFederatedOriginVerifier>();
        verifier.VerifyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new string('b', 64));
        Guid journalId;
        await using (var reserve = database.Open()) journalId = (await new MicrosoftIdentityMigrationService(reserve, verifier, TimeProvider.System)
            .ReserveAsync(source.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new string('a', 64))).Id;
        await using (var apply = database.Open()) await new MicrosoftIdentityMigrationService(apply, verifier, TimeProvider.System).ChangeAsync(journalId, 0, false);
        await using (var business = database.Open())
        {
            var user = await business.Users.SingleAsync();
            user.UpdateDetails(new("After", "qa_after_cutover", null, null, "es", "name"), user.ProfileVersion);
            user.SetSuspended(true, DateTimeOffset.UtcNow);
            business.Groups.Add(new Group("Created after cutover", user.Id));
            await business.SaveChangesAsync();
        }
        await using (var rollback = database.Open()) await new MicrosoftIdentityMigrationService(rollback, verifier, TimeProvider.System).ChangeAsync(journalId, 1, true);
        await using (var retry = database.Open()) Assert.Equal(2, (await new MicrosoftIdentityMigrationService(retry, verifier, TimeProvider.System).ChangeAsync(journalId, 1, true)).Version);
        await using var verify = database.Open();
        var restored = await verify.Users.SingleAsync();
        Assert.Equal(source.Id, restored.Id); Assert.Equal(source.ObjectId, restored.ObjectId);
        Assert.Equal("After", restored.DisplayName); Assert.Equal("qa_after_cutover", restored.Username);
        Assert.Equal("name", restored.DefaultListSort); Assert.True(restored.IsSuspended);
        Assert.Equal(source.Id, (await verify.Groups.SingleAsync()).OwnerId);
        Assert.Equal(source.Id, (await verify.GroupMemberships.SingleAsync()).UserId);
        Assert.Equal(1, await verify.Users.CountAsync());
    }

    [PostgresFact]
    public async Task ConcurrentUsernameEdits_EnforceUniqueIndexAcrossDifferentUsers()
    {
        await using var database = await TestDatabase.CreateAsync();
        var first = new User("First", null, "ciam", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null);
        var second = new User("Second", null, "ciam", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null);
        await using (var seed = database.Open()) { seed.Users.AddRange(first, second); await seed.SaveChangesAsync(); }
        var outcomes = await Task.WhenAll(new[] { first.Id, second.Id }.Select(async id => {
            await using var db = database.Open();
            var user = await db.Users.SingleAsync(x => x.Id == id);
            user.UpdateDetails(new(user.DisplayName, "same_username", null, null, null, "newest"), 0);
            try { await new UserRepository(db).UpdateAsync(user); return true; }
            catch (UserConflictException) { return false; }
        }));
        Assert.Single(outcomes.Where(x => x));
        await using var verify = database.Open();
        Assert.Equal(2, await verify.Users.CountAsync());
        Assert.Equal(1, await verify.Users.CountAsync(x => x.Username == "same_username"));
    }
    [PostgresFact]
    public async Task ConcurrentProvisioning_ConvergesOnOneCanonicalUser()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tid = Guid.NewGuid().ToString(); var oid = Guid.NewGuid().ToString();
        var ids = await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ => {
            await using var db = database.Open();
            return await new SyncProfileCommandHandler(new UserRepository(db)).Handle(new("ciam", tid, oid, null, "QA", null), default);
        }));
        Assert.Single(ids.Select(x => x.UserId).Distinct());
        await using var verify = database.Open(); Assert.Equal(1, await verify.Users.CountAsync());
        Assert.False(verify.Database.HasPendingModelChanges());
    }

    [PostgresFact]
    public async Task ReservationCutoverAndRollback_KeepSameUserAndMembershipReferences()
    {
        await using var database = await TestDatabase.CreateAsync();
        var user = new User("Owned profile", null, "entra", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "legacy");
        var targetTid = Guid.NewGuid().ToString(); var targetOid = Guid.NewGuid().ToString();
        var group = new Group("QA group", user.Id);
        await using (var seed = database.Open()) { seed.Users.Add(user); seed.Groups.Add(group); await seed.SaveChangesAsync(); }
        var verifier = Substitute.For<IFederatedOriginVerifier>();
        verifier.VerifyAsync(user.TenantId!, user.ObjectId!, targetTid, targetOid, Arg.Any<CancellationToken>()).Returns(new string('b',64));
        Guid id;
        await using (var db = database.Open())
        {
            var service = new MicrosoftIdentityMigrationService(db, verifier, TimeProvider.System);
            var reserved = await service.ReserveAsync(user.Id, targetTid, targetOid, new string('a', 64)); id = reserved.Id;
        }
        await using (var blocked = database.Open())
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new SyncProfileCommandHandler(new UserRepository(blocked))
                .Handle(new("ciam", targetTid, targetOid, null, "Do not duplicate", null), default));
        await using (var db = database.Open()) await new MicrosoftIdentityMigrationService(db, verifier, TimeProvider.System).ChangeAsync(id, 0, false);
        await using (var verify = database.Open())
        {
            Assert.Equal(user.Id, (await verify.Users.SingleAsync()).Id);
            Assert.Equal(targetOid, (await verify.Users.SingleAsync()).ObjectId);
            Assert.Equal(user.Id, (await verify.Groups.SingleAsync()).OwnerId);
            Assert.Equal(user.Id, (await verify.GroupMemberships.SingleAsync()).UserId);
            Assert.Equal("Owned profile", (await verify.Users.SingleAsync()).DisplayName);
            Assert.Equal(1, await verify.Users.CountAsync());
            Assert.True(await new UserRepository(verify).IsIdentityReservedAsync("entra", user.TenantId!, user.ObjectId!));
        }
        // Idempotent retry after a lost response; no second User, cut or mutation.
        await using (var retry = database.Open()) Assert.Equal(1, (await new MicrosoftIdentityMigrationService(retry, verifier, TimeProvider.System).ChangeAsync(id, 0, false)).Version);
        await using (var db = database.Open()) await new MicrosoftIdentityMigrationService(db, verifier, TimeProvider.System).ChangeAsync(id, 1, true);
        await using (var verify = database.Open())
        {
            var restored = await verify.Users.SingleAsync();
            Assert.Equal(user.Id, restored.Id); Assert.Equal(user.ObjectId, restored.ObjectId);
            Assert.Equal(user.Id, (await verify.Groups.SingleAsync()).OwnerId);
            Assert.Equal(user.Id, (await verify.GroupMemberships.SingleAsync()).UserId);
            Assert.True(restored.SecurityVersion >= 2);
            Assert.True(await new UserRepository(verify).IsIdentityReservedAsync("ciam", targetTid, targetOid));
        }
    }

    [PostgresFact]
    public async Task OccupiedDestinationOrUnprovenOrigin_CannotReserveOrChangeUser()
    {
        await using var database = await TestDatabase.CreateAsync();
        var source = new User("Source", null, "entra", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null);
        var target = new User("Target", null, "ciam", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null);
        await using (var seed = database.Open()) { seed.Users.AddRange(source, target); await seed.SaveChangesAsync(); }
        var verifier = Substitute.For<IFederatedOriginVerifier>();
        verifier.VerifyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new string('b', 64));
        await using (var db = database.Open()) await Assert.ThrowsAsync<UserConflictException>(() =>
            new MicrosoftIdentityMigrationService(db, verifier, TimeProvider.System).ReserveAsync(source.Id, target.TenantId!, target.ObjectId!, new string('a', 64)));
        verifier.VerifyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(new InvalidOperationException("Unproven origin")));
        await using (var db = database.Open()) await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MicrosoftIdentityMigrationService(db, verifier, TimeProvider.System).ReserveAsync(source.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new string('a', 64)));
        await using var verify = database.Open(); Assert.Equal(2, await verify.Users.CountAsync()); Assert.Empty(await verify.IdentityMigrations.ToListAsync());
    }
}

internal sealed class TestDatabase(string admin, string name, DbContextOptions<UsersDbContext> options) : IAsyncDisposable
{
    public UsersDbContext Open() => new(options);
    public static async Task<TestDatabase> CreateAsync()
    {
        var admin = Environment.GetEnvironmentVariable("MIKOMPRI_TEST_POSTGRES")!;
        var name = "users010a_" + Guid.NewGuid().ToString("N");
        await using var connection = new NpgsqlConnection(admin); await connection.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection)) await create.ExecuteNonQueryAsync();
        var target = new NpgsqlConnectionStringBuilder(admin) { Database = name };
        var result = new TestDatabase(admin, name, new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(target.ConnectionString).Options);
        await using var db = result.Open(); await db.Database.MigrateAsync();
        return result;
    }
    public async ValueTask DisposeAsync()
    {
        // Only a UUID database created by this fixture; never a configured existing database.
        if (!name.StartsWith("users010a_", StringComparison.Ordinal) || name.Length != 42) throw new InvalidOperationException();
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(admin); await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE \"{name}\" WITH (FORCE)", connection); await drop.ExecuteNonQueryAsync();
    }
}
