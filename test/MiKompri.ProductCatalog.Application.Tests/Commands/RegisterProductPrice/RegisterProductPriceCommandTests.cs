using Moq;
using MiKompri.ProductCatalog.Application.Commands.RegisterProductPrice;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Tests.Commands.RegisterProductPrice;

public class RegisterProductPriceCommandTests
{
    [Fact]
    public async Task Handler_Should_Register_ProductPrice_And_SaveChanges()
    {
        var catalogRepositoryMock = new Mock<ICatalogProductRepository>();
        var marketRepositoryMock = new Mock<IMarketRepository>();
        var productPriceRepositoryMock = new Mock<IProductPriceRecordRepository>();
        var dateTimeProviderMock = new Mock<Abstractions.IDateTimeProvider>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var product = new Domain.Products.CatalogProduct(new Domain.Products.ValueObjects.ProductName("Leche"), "l");
        var market = new Domain.Markets.Market("Mercado Central", null);

        catalogRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        marketRepositoryMock
            .Setup(x => x.GetByIdAsync(market.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(market);

        productPriceRepositoryMock
            .Setup(x => x.ExistsAsync(product.Id, market.Id, new DateOnly(2026, 8, 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        dateTimeProviderMock
            .SetupGet(x => x.UtcToday)
            .Returns(new DateOnly(2026, 8, 5));

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RegisterProductPriceCommandHandler(
            catalogRepositoryMock.Object,
            marketRepositoryMock.Object,
            productPriceRepositoryMock.Object,
            dateTimeProviderMock.Object,
            unitOfWorkMock.Object);

        var command = new RegisterProductPriceCommand(product.Id, market.Id, new DateOnly(2026, 8, 5), 1.45m);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        productPriceRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Products.ProductPriceRecord>(), It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Fail_When_PriceAmount_Is_Not_Positive()
    {
        var validator = new RegisterProductPriceCommandValidator();
        var command = new RegisterProductPriceCommand(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 5), 0m);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
