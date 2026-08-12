using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Configurations.SharedLists
{
    public class SharedListAuditEventConfiguration : IEntityTypeConfiguration<SharedListAuditEvent>
    {
        public void Configure(EntityTypeBuilder<SharedListAuditEvent> builder)
        {
            builder.ToTable("shared_list_audit_events");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.SharedPurchaseListId).IsRequired();
            builder.Property(x => x.ActorUserId).IsRequired();
            builder.Property(x => x.ActionType).HasMaxLength(80).IsRequired();
            builder.Property(x => x.TargetEntityType).HasMaxLength(80).IsRequired();
            builder.Property(x => x.TargetEntityId).IsRequired();
            builder.Property(x => x.OccurredAt).IsRequired();
            builder.Property(x => x.Metadata).HasMaxLength(2000);
            builder.HasIndex(x => x.SharedPurchaseListId);
        }
    }
}
