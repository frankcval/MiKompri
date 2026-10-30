namespace MiKompri.Users.Domain.Users;

// A bounded Microsoft transport migration, not an inventory of login methods.
public sealed class IdentityMigration
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string SourceProvider { get; private set; } = "";
    public string SourceTenantId { get; private set; } = "";
    public string SourceObjectId { get; private set; } = "";
    public string TargetProvider { get; private set; } = "";
    public string TargetTenantId { get; private set; } = "";
    public string TargetObjectId { get; private set; } = "";
    public string ApprovalDigest { get; private set; } = "";
    public string EvidenceDigest { get; private set; } = "";
    public string State { get; private set; } = "Reserved";
    public long Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    private IdentityMigration() { }
    public IdentityMigration(User user, string targetProvider, string targetTenantId, string targetObjectId,
        string approvalDigest, string evidenceDigest, DateTimeOffset now)
    {
        if (user.TenantId is null || user.ObjectId is null || user.IdentityProvider != "entra" || targetProvider != "ciam" ||
            !Guid.TryParse(user.TenantId, out _) || !Guid.TryParse(user.ObjectId, out _) ||
            !Guid.TryParse(targetTenantId, out _) || !Guid.TryParse(targetObjectId, out _) ||
            !DigestValid(approvalDigest) || !DigestValid(evidenceDigest))
            throw new InvalidOperationException("La transición requiere origen Microsoft canónico y evidencia aprobada.");
        UserId = user.Id;
        SourceProvider = user.IdentityProvider; SourceTenantId = user.TenantId; SourceObjectId = user.ObjectId;
        TargetProvider = targetProvider; TargetTenantId = targetTenantId; TargetObjectId = targetObjectId;
        ApprovalDigest = approvalDigest; EvidenceDigest = evidenceDigest;
        CreatedAt = UpdatedAt = now.UtcDateTime;
    }
    public void Apply(User user, long version, DateTimeOffset now)
    {
        if (user.Id != UserId || Version != version || user.IsSuspended) throw new InvalidOperationException("Transición no autorizada.");
        if (State == "Applied") return;
        RequireTuple(user, SourceProvider, SourceTenantId, SourceObjectId);
        user.MoveCanonicalIdentity(TargetProvider, TargetTenantId, TargetObjectId, now);
        State = "Applied"; Version++; UpdatedAt = now.UtcDateTime;
    }
    public void Rollback(User user, long version, DateTimeOffset now)
    {
        if (user.Id != UserId || Version != version) throw new InvalidOperationException("Transición no autorizada.");
        if (State == "RolledBack") return;
        if (State != "Applied") throw new InvalidOperationException("No hay cutover aplicado.");
        RequireTuple(user, TargetProvider, TargetTenantId, TargetObjectId);
        user.MoveCanonicalIdentity(SourceProvider, SourceTenantId, SourceObjectId, now);
        State = "RolledBack"; Version++; UpdatedAt = now.UtcDateTime;
    }
    private static void RequireTuple(User user, string provider, string tenant, string oid)
    {
        if (user.IdentityProvider != provider || user.TenantId != tenant || user.ObjectId != oid)
            throw new InvalidOperationException("La identidad origen ha cambiado.");
    }
    private static bool DigestValid(string digest) => digest.Length == 64 && digest.All(Uri.IsHexDigit);
}
