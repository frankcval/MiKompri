using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.Users.Domain.Users;
namespace MiKompri.Users.Infrastructure.Persistence.Configurations;
public sealed class IdentityMigrationConfiguration : IEntityTypeConfiguration<IdentityMigration>
{
    public void Configure(EntityTypeBuilder<IdentityMigration> builder)
    {
        builder.ToTable("IdentityMigrations");
        builder.HasKey(x => x.Id);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => new { x.SourceProvider, x.SourceTenantId, x.SourceObjectId }).IsUnique();
        builder.HasIndex(x => new { x.TargetProvider, x.TargetTenantId, x.TargetObjectId }).IsUnique();
        builder.Property(x => x.Version).IsConcurrencyToken();
        foreach (var name in new[] { "SourceProvider", "SourceTenantId", "SourceObjectId", "TargetProvider", "TargetTenantId", "TargetObjectId" })
            builder.Property<string>(name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.State).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ApprovalDigest).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EvidenceDigest).HasMaxLength(64).IsRequired();
    }
}
