using Moq;
using MiKompri.ProductCatalog.Application.Commands.DeactivateCatalogProduct;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Tests.Commands.DeactivateCatalogProduct;

public class DeactivateCatalogProductCommandTests
{
    [Fact]
    public async Task Handler_Should_Deactivate_Product_And_SaveChanges()
    {
        var repositoryMock = new Mock<ICatalogProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var product = new Domain.Products.CatalogProduct(new Domain.Products.ValueObjects.ProductName("Leche"), "l");

        repositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeactivateCatalogProductCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);
        var command = new DeactivateCatalogProductCommand(product.Id);

        await handler.Handle(command, CancellationToken.None);

        Assert.False(product.IsActive);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Fail_When_Id_Is_Empty()
    {
        var validator = new DeactivateCatalogProductCommandValidator();
        var command = new DeactivateCatalogProductCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
