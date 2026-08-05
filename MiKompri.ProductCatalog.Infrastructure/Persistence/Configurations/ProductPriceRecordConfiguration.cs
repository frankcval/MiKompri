using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Infrastructure.Persistence.Configurations;

public class ProductPriceRecordConfiguration : IEntityTypeConfiguration<ProductPriceRecord>
{
    public void Configure(EntityTypeBuilder<ProductPriceRecord> builder)
    {
        builder.ToTable("ProductPriceRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CatalogProductId)
            .IsRequired();

        builder.Property(x => x.MarketId)
            .IsRequired();

        builder.Property(x => x.EffectiveDate)
            .IsRequired();

        builder.OwnsOne(x => x.Price, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            money.Property(x => x.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.UpdatedBy);

        builder.HasIndex(x => new { x.CatalogProductId, x.MarketId, x.EffectiveDate })
            .HasDatabaseName("UX_ProductPriceRecords_Product_Market_EffectiveDate")
            .IsUnique();

        builder.HasIndex(x => x.CatalogProductId)
            .HasDatabaseName("IX_ProductPriceRecords_CatalogProductId");

        builder.HasIndex(x => x.MarketId)
            .HasDatabaseName("IX_ProductPriceRecords_MarketId");

        builder.HasIndex(x => x.EffectiveDate)
            .HasDatabaseName("IX_ProductPriceRecords_EffectiveDate");

        builder.HasOne<MiKompri.ProductCatalog.Domain.Products.CatalogProduct>()
            .WithMany()
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MiKompri.ProductCatalog.Domain.Markets.Market>()
            .WithMany()
            .HasForeignKey(x => x.MarketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
