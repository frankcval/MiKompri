using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Guardamos el enum GroupRole como string o int.
            // Yo te recomiendo string para legibilidad, pero puedes usar int si prefieres.
            builder.Property(m => m.Role)
                .HasConversion<string>()   // enum ↔ string
                .HasMaxLength(50)
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
