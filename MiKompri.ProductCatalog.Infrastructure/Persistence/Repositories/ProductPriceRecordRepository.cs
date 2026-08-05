using Microsoft.EntityFrameworkCore;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Domain.Products;

namespace MiKompri.ProductCatalog.Infrastructure.Persistence.Repositories;

public class ProductPriceRecordRepository : IProductPriceRecordRepository
{
    private readonly ProductCatalogDbContext _context;

    public ProductPriceRecordRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ProductPriceRecord record, CancellationToken cancellationToken = default)
    {
        await _context.ProductPriceRecords.AddAsync(record, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid catalogProductId, Guid marketId, DateOnly effectiveDate, CancellationToken cancellationToken = default)
    {
        return _context.ProductPriceRecords.AnyAsync(
            x => x.CatalogProductId == catalogProductId &&
                 x.MarketId == marketId &&
                 x.EffectiveDate == effectiveDate,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductPriceRecord>> GetByProductAsync(
        Guid catalogProductId,
        Guid? marketId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ProductPriceRecord> query = _context.ProductPriceRecords
            .Where(x => x.CatalogProductId == catalogProductId);

        if (marketId.HasValue)
        {
            query = query.Where(x => x.MarketId == marketId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.EffectiveDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.EffectiveDate <= to.Value);
        }

        return await query
            .OrderBy(x => x.EffectiveDate)
            .ToListAsync(cancellationToken);
    }
}
