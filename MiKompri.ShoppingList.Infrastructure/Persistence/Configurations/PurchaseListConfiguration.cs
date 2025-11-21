using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Configurations
{
    public class PurchaseListConfiguration : IEntityTypeConfiguration<PurchaseList>
    {
        public void Configure(EntityTypeBuilder<PurchaseList> builder)
        {
            builder.ToTable("purchase_lists");
            builder.HasKey(x => x.Id);
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(p => p.OwnerId)
                .IsRequired();
            builder.Property(p => p.CreatedAt)
                .IsRequired();
            builder.HasMany(p => p.Items)
              .WithOne(i => i.PurchaseList)
              .HasForeignKey(i => i.PurchaseListId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
