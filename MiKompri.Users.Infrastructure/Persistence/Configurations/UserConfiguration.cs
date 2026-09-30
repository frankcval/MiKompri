using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.DisplayName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasMaxLength(200);

            builder.Property(u => u.IdentityProvider)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.ExternalUserId)
                .HasMaxLength(200);

            builder.Property(u => u.TenantId)
                .HasMaxLength(100);

            builder.Property(u => u.ObjectId)
                .HasMaxLength(100);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .IsRequired();

            // Índice único legacy (IdentityProvider, ExternalUserId), solo para filas con ExternalUserId
            builder.HasIndex(u => new { u.IdentityProvider, u.ExternalUserId })
                .IsUnique()
                .HasFilter("\"ExternalUserId\" IS NOT NULL");

            // Índice único canónico (TenantId, ObjectId), solo para filas ya correlacionadas
            builder.HasIndex(u => new { u.TenantId, u.ObjectId })
                .IsUnique()
                .HasFilter("\"TenantId\" IS NOT NULL AND \"ObjectId\" IS NOT NULL");

            // Relación con memberships (un usuario tiene muchas memberships)
            builder.HasMany<GroupMembership>()
                .WithOne()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
