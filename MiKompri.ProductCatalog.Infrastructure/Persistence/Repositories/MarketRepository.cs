using Microsoft.EntityFrameworkCore;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Domain.Markets;

namespace MiKompri.ProductCatalog.Infrastructure.Persistence.Repositories;

public class MarketRepository : IMarketRepository
{
    private readonly ProductCatalogDbContext _context;

    public MarketRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Market market, CancellationToken cancellationToken = default)
    {
        await _context.Markets.AddAsync(market, cancellationToken);
    }

    public Task<Market?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Markets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Market>> GetAsync(bool includeInactive, string? search, CancellationToken cancellationToken = default)
    {
        IQueryable<Market> query = _context.Markets;

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(x => x.Name.Contains(normalizedSearch));
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveByNameAsync(string normalizedName, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var trimmed = normalizedName.Trim();

        IQueryable<Market> query = _context.Markets
            .Where(x => x.IsActive && x.Name == trimmed);

        if (excludedId.HasValue)
        {
            query = query.Where(x => x.Id != excludedId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
