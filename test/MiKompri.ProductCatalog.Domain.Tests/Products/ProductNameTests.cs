using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Domain.Tests.Products;

public class ProductNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldNormalizeAndTrim()
    {
        var name = new ProductName("  Leche Entera  ");

        Assert.Equal("Leche Entera", name.Value);
        Assert.Equal("LECHE ENTERA", name.NormalizedValue);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new ProductName("   "));
    }

    [Fact]
    public void Create_WithMoreThan120Chars_ShouldThrowDomainValidationException()
    {
        var text = new string('A', 121);

        Assert.Throws<DomainValidationException>(() => new ProductName(text));
    }

    [Fact]
    public void Equality_WithDifferentCase_ShouldBeEqual()
    {
        var a = new ProductName("arroz");
        var b = new ProductName("ARROZ");

        Assert.Equal(a, b);
    }
}
