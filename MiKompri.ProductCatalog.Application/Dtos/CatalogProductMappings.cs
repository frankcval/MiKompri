using MiKompri.ProductCatalog.Domain.Products;

namespace MiKompri.ProductCatalog.Application.Dtos;

internal static class CatalogProductMappings
{
    public static CatalogProductDto ToDto(this CatalogProduct product)
    {
        return new CatalogProductDto
        {
            Id = product.Id,
            Name = product.Name.Value,
            PurchaseUnit = product.PurchaseUnit,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
