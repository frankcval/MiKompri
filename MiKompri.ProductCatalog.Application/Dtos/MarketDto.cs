namespace MiKompri.ProductCatalog.Application.Dtos;

public class MarketDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocationHint { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
