using MiKompri.ProductCatalog.Domain.Exceptions;
using MiKompri.ProductCatalog.Domain.Markets;

namespace MiKompri.ProductCatalog.Domain.Tests.Markets;

public class MarketTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializeAsActive()
    {
        var market = new Market("Mercado Central", "Av. Principal 123");

        Assert.True(market.IsActive);
        Assert.Equal("Mercado Central", market.Name);
        Assert.Equal("Av. Principal 123", market.LocationHint);
        Assert.True(market.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(market.CreatedAt, market.UpdatedAt);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new Market("  ", "Ubicación"));
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndLocation()
    {
        var market = new Market("Mercado A", "Zona Norte");
        var before = market.UpdatedAt;

        Thread.Sleep(5);
        market.UpdateDetails("Mercado B", "Zona Sur");

        Assert.Equal("Mercado B", market.Name);
        Assert.Equal("Zona Sur", market.LocationHint);
        Assert.True(market.UpdatedAt > before);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeIdempotent()
    {
        var market = new Market("Mercado A", null);

        market.Deactivate();
        var before = market.UpdatedAt;

        Thread.Sleep(5);
        market.Deactivate();

        Assert.False(market.IsActive);
        Assert.Equal(before, market.UpdatedAt);
    }
}
