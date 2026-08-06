using System.ComponentModel.DataAnnotations;

namespace MiKompri.ProductCatalog.Application.Options;

public class ProductCatalogOptions
{
    public const string SectionName = "ProductCatalog";

    [Required]
    [MinLength(1)]
    public string Currency { get; set; } = "EUR";
}
