using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MiKompri.ProductCatalog.Api.Controllers;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Queries.GetProductPriceHistory;

namespace MiKompri.ProductCatalog.Api.Tests;

public class CatalogProductsPriceHistoryApiTests
{
    [Fact]
    public async Task GetPriceHistory_Should_Return_Ok_With_History()
    {
        var mediatorMock = new Mock<IMediator>();
        var productId = Guid.NewGuid();
        var marketId = Guid.NewGuid();

        var dto = new ProductPriceHistoryDto
        {
            CatalogProductId = productId,
            CatalogProductName = "Leche Entera",
            Records = new List<ProductPriceHistoryRecordDto>
            {
                new()
                {
                    MarketId = marketId,
                    MarketName = "Mercado Central",
                    EffectiveDate = new DateOnly(2026, 8, 1),
                    PriceAmount = 1.35m,
                    Currency = "EUR"
                }
            }
        };

        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetProductPriceHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controller = new CatalogProductsController(mediatorMock.Object);

        var response = await controller.GetPriceHistory(productId, null, null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var value = Assert.IsType<ProductPriceHistoryDto>(ok.Value);
        Assert.Equal(productId, value.CatalogProductId);
        Assert.Single(value.Records);
    }
}
