using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Configurations.SharedLists
{
    public class ItemExpenseRecordConfiguration : IEntityTypeConfiguration<ItemExpenseRecord>
    {
        public void Configure(EntityTypeBuilder<ItemExpenseRecord> builder)
        {
            builder.ToTable("shared_list_item_expenses");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.SharedListItemId).IsRequired();
            builder.Property(x => x.PaidBy).IsRequired();
            builder.Property(x => x.RealPaidPrice).HasColumnType("decimal(12,2)").IsRequired();
            builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasOne<ListItem>()
                .WithMany(x => x.Expenses)
                .HasForeignKey(x => x.SharedListItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Participants)
                .WithOne()
                .HasForeignKey(x => x.ItemExpenseRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
