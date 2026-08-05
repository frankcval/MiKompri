using MiKompri.ProductCatalog.Domain.Products;

namespace MiKompri.ProductCatalog.Application.Interfaces;

public interface IProductPriceRecordRepository
{
    Task AddAsync(ProductPriceRecord record, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid catalogProductId, Guid marketId, DateOnly effectiveDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductPriceRecord>> GetByProductAsync(
        Guid catalogProductId,
        Guid? marketId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default);
}
