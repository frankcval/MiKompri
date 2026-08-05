using Moq;
using MiKompri.ProductCatalog.Application.Commands.UpdateMarket;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Tests.Commands.UpdateMarket;

public class UpdateMarketCommandTests
{
    [Fact]
    public async Task Handler_Should_Update_Market_And_SaveChanges()
    {
        var repositoryMock = new Mock<IMarketRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var market = new Domain.Markets.Market("Mercado A", "Zona Norte");

        repositoryMock
            .Setup(x => x.GetByIdAsync(market.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(market);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateMarketCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);
        var command = new UpdateMarketCommand(market.Id, "Mercado B", "Zona Sur");

        await handler.Handle(command, CancellationToken.None);

        repositoryMock.Verify(x => x.GetByIdAsync(market.Id, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Fail_When_MarketId_Is_Empty()
    {
        var validator = new UpdateMarketCommandValidator();
        var command = new UpdateMarketCommand(Guid.Empty, "Mercado", "Ubicación");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
