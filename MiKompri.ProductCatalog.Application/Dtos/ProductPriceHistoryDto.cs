namespace MiKompri.ProductCatalog.Application.Dtos;

public class ProductPriceHistoryDto
{
    public Guid CatalogProductId { get; set; }
    public string CatalogProductName { get; set; } = string.Empty;
    public List<ProductPriceHistoryRecordDto> Records { get; set; } = new();
}

public class ProductPriceHistoryRecordDto
{
    public Guid MarketId { get; set; }
    public string MarketName { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
