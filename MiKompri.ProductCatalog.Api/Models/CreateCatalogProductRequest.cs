namespace MiKompri.ProductCatalog.Api.Models;

public class CreateCatalogProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
}
