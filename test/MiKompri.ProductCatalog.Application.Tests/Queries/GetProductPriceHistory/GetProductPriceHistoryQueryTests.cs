using Moq;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Application.Queries.GetProductPriceHistory;
using MiKompri.ProductCatalog.Domain.Markets;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Application.Tests.Queries.GetProductPriceHistory;

public class GetProductPriceHistoryQueryTests
{
    [Fact]
    public async Task Handler_Should_Return_Chronological_History()
    {
        var catalogRepositoryMock = new Mock<ICatalogProductRepository>();
        var marketRepositoryMock = new Mock<IMarketRepository>();
        var priceRepositoryMock = new Mock<IProductPriceRecordRepository>();

        var product = new CatalogProduct(new ProductName("Leche Entera"), "l");
        var market = new Market("Mercado Central", null);

        var first = new ProductPriceRecord(product.Id, market.Id, new DateOnly(2026, 8, 1), new Money(1.35m, "EUR"));
        var second = new ProductPriceRecord(product.Id, market.Id, new DateOnly(2026, 8, 5), new Money(1.45m, "EUR"));

        catalogRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        marketRepositoryMock
            .Setup(x => x.GetByIdAsync(market.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(market);

        priceRepositoryMock
            .Setup(x => x.GetByProductAsync(product.Id, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductPriceRecord> { second, first });

        var handler = new GetProductPriceHistoryQueryHandler(
            catalogRepositoryMock.Object,
            marketRepositoryMock.Object,
            priceRepositoryMock.Object);

        var query = new GetProductPriceHistoryQuery(product.Id, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(product.Id, result.CatalogProductId);
        Assert.Equal("Leche Entera", result.CatalogProductName);
        Assert.Equal(2, result.Records.Count);
        Assert.Equal(new DateOnly(2026, 8, 1), result.Records[0].EffectiveDate);
        Assert.Equal(new DateOnly(2026, 8, 5), result.Records[1].EffectiveDate);
    }

    [Fact]
    public async Task Handler_Should_Return_Empty_Records_When_No_Data()
    {
        var catalogRepositoryMock = new Mock<ICatalogProductRepository>();
        var marketRepositoryMock = new Mock<IMarketRepository>();
        var priceRepositoryMock = new Mock<IProductPriceRecordRepository>();

        var product = new CatalogProduct(new ProductName("Arroz"), "kg");

        catalogRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        priceRepositoryMock
            .Setup(x => x.GetByProductAsync(product.Id, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ProductPriceRecord>());

        var handler = new GetProductPriceHistoryQueryHandler(
            catalogRepositoryMock.Object,
            marketRepositoryMock.Object,
            priceRepositoryMock.Object);

        var query = new GetProductPriceHistoryQuery(product.Id, null, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(product.Id, result.CatalogProductId);
        Assert.Empty(result.Records);
    }
}
