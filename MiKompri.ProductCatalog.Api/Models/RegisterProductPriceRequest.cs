namespace MiKompri.ProductCatalog.Api.Models;

public class RegisterProductPriceRequest
{
    public Guid CatalogProductId { get; set; }
    public Guid MarketId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public decimal PriceAmount { get; set; }
}
