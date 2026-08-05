using Moq;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Application.Queries.GetCatalogProducts;

namespace MiKompri.ProductCatalog.Application.Tests.Queries.GetCatalogProducts;

public class GetCatalogProductsQueryTests
{
    [Fact]
    public async Task Handler_Should_Return_Products_From_Repository()
    {
        var repositoryMock = new Mock<ICatalogProductRepository>();
        var products = new List<Domain.Products.CatalogProduct>
        {
            new(new Domain.Products.ValueObjects.ProductName("Leche"), "l"),
            new(new Domain.Products.ValueObjects.ProductName("Arroz"), "kg")
        };

        repositoryMock
            .Setup(x => x.GetAsync(false, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var handler = new GetCatalogProductsQueryHandler(repositoryMock.Object);
        var query = new GetCatalogProductsQuery(false, null);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
