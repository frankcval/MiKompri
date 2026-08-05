namespace MiKompri.ProductCatalog.Application.Dtos;

public class ProductPriceRecordDto
{
    public Guid Id { get; set; }
    public Guid CatalogProductId { get; set; }
    public Guid MarketId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
