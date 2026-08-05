using Moq;
using MiKompri.ProductCatalog.Application.Commands.UpdateCatalogProduct;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Tests.Commands.UpdateCatalogProduct;

public class UpdateCatalogProductCommandTests
{
    [Fact]
    public async Task Handler_Should_Update_Product_And_SaveChanges()
    {
        var repositoryMock = new Mock<ICatalogProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var product = new Domain.Products.CatalogProduct(new Domain.Products.ValueObjects.ProductName("Arroz"), "kg");

        repositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateCatalogProductCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);
        var command = new UpdateCatalogProductCommand(product.Id, "Arroz Integral", "kg");

        await handler.Handle(command, CancellationToken.None);

        repositoryMock.Verify(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Fail_When_PurchaseUnit_Is_Empty()
    {
        var validator = new UpdateCatalogProductCommandValidator();
        var command = new UpdateCatalogProductCommand(Guid.NewGuid(), "Arroz", " ");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
