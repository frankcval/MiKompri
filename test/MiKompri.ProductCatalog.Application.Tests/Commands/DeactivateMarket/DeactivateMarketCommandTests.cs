using Moq;
using MiKompri.ProductCatalog.Application.Commands.DeactivateMarket;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Tests.Commands.DeactivateMarket;

public class DeactivateMarketCommandTests
{
    [Fact]
    public async Task Handler_Should_Deactivate_Market_And_SaveChanges()
    {
        var repositoryMock = new Mock<IMarketRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var market = new Domain.Markets.Market("Mercado A", null);

        repositoryMock
            .Setup(x => x.GetByIdAsync(market.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(market);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeactivateMarketCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);
        var command = new DeactivateMarketCommand(market.Id);

        await handler.Handle(command, CancellationToken.None);

        Assert.False(market.IsActive);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Fail_When_MarketId_Is_Empty()
    {
        var validator = new DeactivateMarketCommandValidator();
        var command = new DeactivateMarketCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
