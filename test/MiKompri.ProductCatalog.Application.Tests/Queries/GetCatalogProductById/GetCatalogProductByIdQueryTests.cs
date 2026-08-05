using Moq;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Application.Queries.GetCatalogProductById;

namespace MiKompri.ProductCatalog.Application.Tests.Queries.GetCatalogProductById;

public class GetCatalogProductByIdQueryTests
{
    [Fact]
    public async Task Handler_Should_Return_Product_When_Exists()
    {
        var repositoryMock = new Mock<ICatalogProductRepository>();
        var product = new Domain.Products.CatalogProduct(new Domain.Products.ValueObjects.ProductName("Leche"), "l");

        repositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new GetCatalogProductByIdQueryHandler(repositoryMock.Object);
        var query = new GetCatalogProductByIdQuery(product.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
    }
}
