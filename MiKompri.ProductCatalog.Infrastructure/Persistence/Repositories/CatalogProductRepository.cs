using Microsoft.EntityFrameworkCore;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Domain.Products;

namespace MiKompri.ProductCatalog.Infrastructure.Persistence.Repositories;

public class CatalogProductRepository : ICatalogProductRepository
{
    private readonly ProductCatalogDbContext _context;

    public CatalogProductRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CatalogProduct product, CancellationToken cancellationToken = default)
    {
        await _context.CatalogProducts.AddAsync(product, cancellationToken);
    }

    public Task<CatalogProduct?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.CatalogProducts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CatalogProduct>> GetAsync(bool includeInactive, string? search, CancellationToken cancellationToken = default)
    {
        IQueryable<CatalogProduct> query = _context.CatalogProducts;

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.NormalizedName.Contains(normalizedSearch));
        }

        return await query
            .OrderBy(x => x.NormalizedName)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveWithNameAndUnitAsync(
        string normalizedName,
        string purchaseUnit,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedUnit = purchaseUnit.Trim();

        IQueryable<CatalogProduct> query = _context.CatalogProducts
            .Where(x => x.IsActive &&
                        x.NormalizedName == normalizedName &&
                        x.PurchaseUnit == normalizedUnit);

        if (excludedId.HasValue)
        {
            query = query.Where(x => x.Id != excludedId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
