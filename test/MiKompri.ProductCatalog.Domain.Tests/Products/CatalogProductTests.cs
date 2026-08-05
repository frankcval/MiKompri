using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Domain.Tests.Products;

public class CatalogProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializeAsActive()
    {
        var name = new ProductName("Leche Entera");

        var product = new CatalogProduct(name, "l");

        Assert.True(product.IsActive);
        Assert.Equal("l", product.PurchaseUnit);
        Assert.Equal("LECHE ENTERA", product.NormalizedName);
        Assert.Equal(name, product.Name);
        Assert.True(product.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(product.CreatedAt, product.UpdatedAt);
    }

    [Fact]
    public void Create_WithEmptyPurchaseUnit_ShouldThrowDomainValidationException()
    {
        var name = new ProductName("Leche");

        Assert.Throws<DomainValidationException>(() => new CatalogProduct(name, "   "));
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateValuesAndUpdatedAt()
    {
        var product = new CatalogProduct(new ProductName("Leche"), "l");
        var previousUpdatedAt = product.UpdatedAt;

        Thread.Sleep(5);
        product.UpdateDetails(new ProductName("Leche Deslactosada"), "unit");

        Assert.Equal("Leche Deslactosada", product.Name.Value);
        Assert.Equal("LECHE DESLACTOSADA", product.NormalizedName);
        Assert.Equal("unit", product.PurchaseUnit);
        Assert.True(product.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveFalseAndUpdateTimestamp()
    {
        var product = new CatalogProduct(new ProductName("Arroz"), "kg");
        var previousUpdatedAt = product.UpdatedAt;

        Thread.Sleep(5);
        product.Deactivate();

        Assert.False(product.IsActive);
        Assert.True(product.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeIdempotent()
    {
        var product = new CatalogProduct(new ProductName("Arroz"), "kg");

        product.Deactivate();
        var previousUpdatedAt = product.UpdatedAt;

        Thread.Sleep(5);
        product.Deactivate();

        Assert.False(product.IsActive);
        Assert.Equal(previousUpdatedAt, product.UpdatedAt);
    }
}
