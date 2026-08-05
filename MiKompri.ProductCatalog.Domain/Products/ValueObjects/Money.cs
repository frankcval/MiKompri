using MiKompri.ProductCatalog.Domain.Abstractions;
using MiKompri.ProductCatalog.Domain.Exceptions;

namespace MiKompri.ProductCatalog.Domain.Products.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount <= 0)
        {
            throw new DomainValidationException("El precio debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainValidationException("La moneda es obligatoria.");
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3)
        {
            throw new DomainValidationException("La moneda debe tener exactamente 3 caracteres.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = normalizedCurrency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
