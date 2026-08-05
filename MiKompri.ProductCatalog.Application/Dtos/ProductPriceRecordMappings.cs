using MiKompri.ProductCatalog.Domain.Products;

namespace MiKompri.ProductCatalog.Application.Dtos;

internal static class ProductPriceRecordMappings
{
    public static ProductPriceRecordDto ToDto(this ProductPriceRecord record)
    {
        return new ProductPriceRecordDto
        {
            Id = record.Id,
            CatalogProductId = record.CatalogProductId,
            MarketId = record.MarketId,
            EffectiveDate = record.EffectiveDate,
            PriceAmount = record.Price.Amount,
            Currency = record.Price.Currency
        };
    }
}
