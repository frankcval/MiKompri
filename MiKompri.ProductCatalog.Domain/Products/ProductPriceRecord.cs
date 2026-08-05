using MiKompri.ProductCatalog.Domain.Abstractions;
using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Domain.Products;

public class ProductPriceRecord : Entity
{
    public Guid CatalogProductId { get; private set; }
    public Guid MarketId { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public Money Price { get; private set; } = null!;

    private ProductPriceRecord()
    {
    }

    public ProductPriceRecord(Guid catalogProductId, Guid marketId, DateOnly effectiveDate, Money price)
    {
        if (catalogProductId == Guid.Empty)
        {
            throw new DomainValidationException("catalogProductId es obligatorio.");
        }

        if (marketId == Guid.Empty)
        {
            throw new DomainValidationException("marketId es obligatorio.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (effectiveDate > today)
        {
            throw new DomainValidationException("effectiveDate no puede ser futura.");
        }

        ArgumentNullException.ThrowIfNull(price);

        CatalogProductId = catalogProductId;
        MarketId = marketId;
        EffectiveDate = effectiveDate;
        Price = price;
    }
}
