using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Infrastructure.Persistence.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.ToTable("Groups");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(g => g.OwnerId)
                .IsRequired();

            builder.Property(g => g.CreatedAt)
                .IsRequired();

            builder.Property(g => g.UpdatedAt)
                .IsRequired();

            // Índice para consultas por Owner
            builder.HasIndex(g => g.OwnerId);

            // Relación Group (1) -> (N) GroupMemberships
            builder.HasMany(g => g.Memberships)
                .WithOne()
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
