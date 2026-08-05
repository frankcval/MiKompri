using MiKompri.ProductCatalog.Domain.Abstractions;
using MiKompri.ProductCatalog.Domain.Exceptions;

namespace MiKompri.ProductCatalog.Domain.Products.ValueObjects;

public sealed class ProductName : ValueObject
{
    public string Value { get; }
    public string NormalizedValue { get; }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("El nombre del producto es obligatorio.");
        }

        var trimmed = value.Trim();

        if (trimmed.Length > 120)
        {
            throw new DomainValidationException("El nombre del producto no puede superar 120 caracteres.");
        }

        Value = trimmed;
        NormalizedValue = trimmed.ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return NormalizedValue;
    }
}
