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
                .HasMaxLength(200)
                .IsRequired();

            // Índice único para (IdentityProvider, ExternalUserId)
            builder.HasIndex(u => new { u.IdentityProvider, u.ExternalUserId })
                .IsUnique();

            // Relación con memberships (un usuario tiene muchas memberships)
            builder.HasMany<GroupMembership>()
                .WithOne() // navegación simple desde GroupMembership
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
