using Moq;
using MiKompri.ProductCatalog.Application.Commands.CreateCatalogProduct;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Tests.Commands.CreateCatalogProduct;

public class CreateCatalogProductCommandTests
{
    [Fact]
    public async Task Handler_Should_Create_Product_And_SaveChanges()
    {
        var repositoryMock = new Mock<ICatalogProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateCatalogProductCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateCatalogProductCommand("Leche Entera", "l");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Products.CatalogProduct>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Fail_When_Name_Is_Empty()
    {
        var validator = new CreateCatalogProductCommandValidator();
        var command = new CreateCatalogProductCommand(" ", "l");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
