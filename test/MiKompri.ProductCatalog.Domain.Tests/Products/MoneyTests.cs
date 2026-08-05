using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Domain.Tests.Products;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldRoundAndNormalize()
    {
        var money = new Money(1.456m, "eur");

        Assert.Equal(1.46m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Create_WithNonPositiveAmount_ShouldThrowDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new Money(0m, "EUR"));
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ShouldThrowDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new Money(10m, "EU"));
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        var a = new Money(10m, "EUR");
        var b = new Money(10.00m, "eur");

        Assert.Equal(a, b);
    }
}
