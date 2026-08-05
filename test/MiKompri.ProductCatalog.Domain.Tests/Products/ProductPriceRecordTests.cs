using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Domain.Tests.Products;

public class ProductPriceRecordTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitialize()
    {
        var record = new ProductPriceRecord(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 5),
            new Money(1.45m, "EUR"));

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
            new Money(1.45m, "EUR")));
    }

    [Fact]
    public void Create_WithEmptyMarketId_ShouldThrowDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new ProductPriceRecord(
            Guid.NewGuid(),
            Guid.Empty,
            new DateOnly(2026, 8, 5),
            new Money(1.45m, "EUR")));
    }

    [Fact]
    public void Create_WithFutureEffectiveDate_ShouldThrowDomainValidationException()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        Assert.Throws<DomainValidationException>(() => new ProductPriceRecord(
            Guid.NewGuid(),
            Guid.NewGuid(),
            futureDate,
            new Money(1.45m, "EUR")));
    }
}
