using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ShoppingList.Domain.Entities;
namespace MiKompri.ShoppingList.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOperationConfiguration : IEntityTypeConfiguration<PurchaseOperation>
{
    public void Configure(EntityTypeBuilder<PurchaseOperation> builder)
    {
        builder.ToTable("purchase_operations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        // Deliberately no FK/cascade to expenses: deletion must not enable replay.
    }
}
