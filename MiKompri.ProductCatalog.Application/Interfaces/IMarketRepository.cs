using MiKompri.ProductCatalog.Domain.Markets;

namespace MiKompri.ProductCatalog.Application.Interfaces;

public interface IMarketRepository
{
    Task AddAsync(Market market, CancellationToken cancellationToken = default);
    Task<Market?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Market>> GetAsync(bool includeInactive, string? search, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByNameAsync(string normalizedName, Guid? excludedId = null, CancellationToken cancellationToken = default);
}
