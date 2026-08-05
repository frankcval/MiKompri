using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Infrastructure.Persistence.Configurations;

public class CatalogProductConfiguration : IEntityTypeConfiguration<CatalogProduct>
{
    public void Configure(EntityTypeBuilder<CatalogProduct> builder)
    {
        builder.ToTable("CatalogProducts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasConversion(
                value => value.Value,
                value => new ProductName(value))
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.PurchaseUnit)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.UpdatedBy);

        builder.HasIndex(x => new { x.NormalizedName, x.PurchaseUnit })
            .HasDatabaseName("UX_CatalogProducts_NormalizedName_PurchaseUnit_Active")
            .HasFilter("\"IsActive\" = TRUE")
            .IsUnique();
    }
}
