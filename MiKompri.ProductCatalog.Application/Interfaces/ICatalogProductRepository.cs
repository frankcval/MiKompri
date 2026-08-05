using MiKompri.ProductCatalog.Domain.Products;

namespace MiKompri.ProductCatalog.Application.Interfaces;

public interface ICatalogProductRepository
{
    Task AddAsync(CatalogProduct product, CancellationToken cancellationToken = default);
    Task<CatalogProduct?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CatalogProduct>> GetAsync(bool includeInactive, string? search, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveWithNameAndUnitAsync(string normalizedName, string purchaseUnit, Guid? excludedId = null, CancellationToken cancellationToken = default);
}
