using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Domain.Users;
using MiKompri.Users.Infrastructure.Persistence;

namespace MiKompri.Users.Infrastructure.Identity;
public sealed class MicrosoftIdentityMigrationService(UsersDbContext db, IFederatedOriginVerifier verifier, TimeProvider clock)
{
    public async Task<IdentityMigration> ReserveAsync(Guid userId, string targetTenant, string targetObject, string approvalDigest, CancellationToken ct = default)
    {
        // Preliminary origin read is revalidated after the tuple locks.
        var source = await db.Users.AsNoTracking().SingleAsync(x => x.Id == userId, ct);
        var evidence = await verifier.VerifyAsync(source.TenantId!, source.ObjectId!, targetTenant, targetObject, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await IdentityTupleLock.AcquireAsync(db, ct, (source.IdentityProvider, source.TenantId!, source.ObjectId!), ("ciam", targetTenant, targetObject));
        var user = await db.Users.SingleAsync(x => x.Id == userId, ct);
        await db.Entry(user).ReloadAsync(ct);
        if (user.IdentityProvider != source.IdentityProvider || user.TenantId != source.TenantId || user.ObjectId != source.ObjectId || user.IsSuspended)
            throw new InvalidOperationException("El origen cambió durante la preparación.");
        var existing = await db.IdentityMigrations.SingleOrDefaultAsync(x => x.UserId == userId, ct);
        if (existing is not null)
        {
            if (existing.TargetTenantId != targetTenant || existing.TargetObjectId != targetObject || existing.ApprovalDigest != approvalDigest || existing.EvidenceDigest != evidence)
                throw new InvalidOperationException("La reserva existente no coincide.");
            return existing;
        }
        if (await db.Users.AnyAsync(x => x.IdentityProvider == "ciam" && x.TenantId == targetTenant && x.ObjectId == targetObject, ct))
            throw new UserConflictException();
        var migration = new IdentityMigration(user, "ciam", targetTenant, targetObject, approvalDigest, evidence, clock.GetUtcNow());
        db.IdentityMigrations.Add(migration);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return migration;
    }

    public async Task<IdentityMigration> ChangeAsync(Guid migrationId, long expectedVersion, bool rollback, CancellationToken ct = default)
    {
        var initial = await db.IdentityMigrations.AsNoTracking().SingleAsync(x => x.Id == migrationId, ct);
        // Recheck immutable origin before cutover. Rollback restores only the approved source,
        // keeping suspension and token cutoffs; it never relies on a matching email.
        if (!rollback) await verifier.VerifyAsync(initial.SourceTenantId, initial.SourceObjectId, initial.TargetTenantId, initial.TargetObjectId, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await IdentityTupleLock.AcquireAsync(db, ct, (initial.SourceProvider, initial.SourceTenantId, initial.SourceObjectId), (initial.TargetProvider, initial.TargetTenantId, initial.TargetObjectId));
        var migration = await db.IdentityMigrations.SingleAsync(x => x.Id == migrationId, ct);
        await db.Entry(migration).ReloadAsync(ct);
        var user = await db.Users.SingleAsync(x => x.Id == migration.UserId, ct);
        await db.Entry(user).ReloadAsync(ct);
        // Idempotent retry only for the already reached state and preceding version.
        var desired = rollback ? "RolledBack" : "Applied";
        if (migration.State == desired && migration.Version == expectedVersion + 1) return migration;
        if (rollback) migration.Rollback(user, expectedVersion, clock.GetUtcNow());
        else migration.Apply(user, expectedVersion, clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return migration;
    }
}
