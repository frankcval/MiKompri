using MiKompri.ProductCatalog.Domain.Abstractions;
using MiKompri.ProductCatalog.Domain.Exceptions;

namespace MiKompri.ProductCatalog.Domain.Markets;

public class Market : Entity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? LocationHint { get; private set; }
    public bool IsActive { get; private set; }

    private Market()
    {
    }

    public Market(string name, string? locationHint)
    {
        Name = NormalizeName(name);
        LocationHint = NormalizeLocationHint(locationHint);
        IsActive = true;
    }

    public void UpdateDetails(string name, string? locationHint, Guid? updatedBy = null)
    {
        var normalizedName = NormalizeName(name);
        var normalizedLocationHint = NormalizeLocationHint(locationHint);

        var changed = Name != normalizedName || LocationHint != normalizedLocationHint;
        if (!changed)
        {
            return;
        }

        Name = normalizedName;
        LocationHint = normalizedLocationHint;
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

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("El nombre del mercado es obligatorio.");
        }

        var normalized = name.Trim();

        if (normalized.Length > 120)
        {
            throw new DomainValidationException("El nombre del mercado no puede superar 120 caracteres.");
        }

        return normalized;
    }

    private static string? NormalizeLocationHint(string? locationHint)
    {
        if (string.IsNullOrWhiteSpace(locationHint))
        {
            return null;
        }

        var normalized = locationHint.Trim();

        if (normalized.Length > 180)
        {
            throw new DomainValidationException("La ubicación descriptiva no puede superar 180 caracteres.");
        }

        return normalized;
    }
}
