using MiKompri.ProductCatalog.Domain.Abstractions;
using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Domain.Products;

public class CatalogProduct : Entity, IAggregateRoot
{
    public ProductName Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = string.Empty;
    public string PurchaseUnit { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private CatalogProduct()
    {
    }

    public CatalogProduct(ProductName name, string purchaseUnit)
    {
        SetName(name);
        PurchaseUnit = NormalizePurchaseUnit(purchaseUnit);
        IsActive = true;
    }

    public void UpdateDetails(ProductName name, string purchaseUnit, Guid? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        var normalizedUnit = NormalizePurchaseUnit(purchaseUnit);
        var changed = Name != name || PurchaseUnit != normalizedUnit;

        if (!changed)
        {
            return;
        }

        SetName(name);
        PurchaseUnit = normalizedUnit;
        Touch(updatedBy);
    }

    public void Deactivate(Guid? updatedBy = null)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(updatedBy);
    }

    private void SetName(ProductName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        NormalizedName = name.NormalizedValue;
    }

    private static string NormalizePurchaseUnit(string purchaseUnit)
    {
        if (string.IsNullOrWhiteSpace(purchaseUnit))
        {
            throw new DomainValidationException("La unidad de compra es obligatoria.");
        }

        var normalizedUnit = purchaseUnit.Trim();

        if (normalizedUnit.Length > 30)
        {
            throw new DomainValidationException("La unidad de compra no puede superar 30 caracteres.");
        }

        return normalizedUnit;
    }
}
