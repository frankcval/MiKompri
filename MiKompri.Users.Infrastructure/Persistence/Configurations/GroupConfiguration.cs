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
