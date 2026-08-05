using Moq;
using MiKompri.ProductCatalog.Application.Commands.CreateMarket;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Tests.Commands.CreateMarket;

public class CreateMarketCommandTests
{
    [Fact]
    public async Task Handler_Should_Create_Market_And_SaveChanges()
    {
        var repositoryMock = new Mock<IMarketRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateMarketCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateMarketCommand("Mercado Central", "Av. Principal 123");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Markets.Market>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Fail_When_Name_Is_Empty()
    {
        var validator = new CreateMarketCommandValidator();
        var command = new CreateMarketCommand(" ", "Ubicación");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
