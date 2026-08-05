namespace MiKompri.ProductCatalog.Api.Models;

public class CreateMarketRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LocationHint { get; set; }
}
