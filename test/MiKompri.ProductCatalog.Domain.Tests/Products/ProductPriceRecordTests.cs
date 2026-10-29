using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Domain.Tests.Products;

public class ProductPriceRecordTests
{
    [Fact]
    public void Create_UsesTheExplicitCivilTodayWithoutReadingTheSystemClock()
    {
        var today = new DateOnly(2030, 10, 30);
        var record = new ProductPriceRecord(Guid.NewGuid(), Guid.NewGuid(), today, new Money(1m, "EUR"), today);
        Assert.Equal(today, record.EffectiveDate);
    }

    [Fact]
    public void Create_WithValidData_ShouldInitialize()
    {
        var record = new ProductPriceRecord(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 5),
            new Money(1.45m, "EUR"), new DateOnly(2030, 10, 30));

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal(1.45m, record.Price.Amount);
        Assert.Equal("EUR", record.Price.Currency);
    }

    [Fact]
    public void Create_WithEmptyCatalogProductId_ShouldThrowDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new ProductPriceRecord(
            Guid.Empty,
            Guid.NewGuid(),
            new DateOnly(2026, 8, 5),
            new Money(1.45m, "EUR"), new DateOnly(2030, 10, 30)));
    }

    [Fact]
    public void Create_WithEmptyMarketId_ShouldThrowDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new ProductPriceRecord(
            Guid.NewGuid(),
            Guid.Empty,
            new DateOnly(2026, 8, 5),
            new Money(1.45m, "EUR"), new DateOnly(2030, 10, 30)));
    }

    [Fact]
    public void Create_WithFutureEffectiveDate_ShouldThrowDomainValidationException()
    {
        var futureDate = new DateOnly(2030, 10, 31);

        Assert.Throws<DomainValidationException>(() => new ProductPriceRecord(
            Guid.NewGuid(),
            Guid.NewGuid(),
            futureDate,
            new Money(1.45m, "EUR"), new DateOnly(2030, 10, 30)));
    }
}
