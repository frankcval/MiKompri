namespace MiKompri.ProductCatalog.Api.Models;

public class UpdateMarketRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LocationHint { get; set; }
}
