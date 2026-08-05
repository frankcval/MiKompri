using Microsoft.EntityFrameworkCore;
using MiKompri.ProductCatalog.Domain.Markets;
using MiKompri.ProductCatalog.Domain.Products;

namespace MiKompri.ProductCatalog.Infrastructure.Persistence;

public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<ProductPriceRecord> ProductPriceRecords => Set<ProductPriceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductCatalogDbContext).Assembly);
    }
}
