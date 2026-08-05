using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiKompri.ProductCatalog.Api.Models;
using MiKompri.ProductCatalog.Application.Commands.RegisterProductPrice;

namespace MiKompri.ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/v1/product-prices")]
public class ProductPricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductPricesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] RegisterProductPriceRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new RegisterProductPriceCommand(
                request.CatalogProductId,
                request.MarketId,
                request.EffectiveDate,
                request.PriceAmount),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, id);
    }
}
