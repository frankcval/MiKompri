using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Infrastructure.Persistence.Configurations
{
    public class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
    {
        public void Configure(EntityTypeBuilder<GroupMembership> builder)
        {
            builder.ToTable("GroupMemberships");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.GroupId)
                .IsRequired();

            builder.Property(m => m.UserId)
                .IsRequired();

            // Enum almacenado como string para legibilidad y seguridad ante reordenación de valores
            builder.Property(m => m.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(m => m.JoinedAt)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .IsRequired();

            // Constraint única: un usuario no puede tener dos memberships en el mismo grupo
            builder.HasIndex(m => new { m.GroupId, m.UserId })
                .IsUnique();

            // Índices para facilitar consultas
            builder.HasIndex(m => m.UserId);
            builder.HasIndex(m => m.GroupId);
        }
    }
}
