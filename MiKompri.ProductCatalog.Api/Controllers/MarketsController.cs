using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiKompri.ProductCatalog.Api.Models;
using MiKompri.ProductCatalog.Application.Commands.CreateMarket;
using MiKompri.ProductCatalog.Application.Commands.DeactivateMarket;
using MiKompri.ProductCatalog.Application.Commands.UpdateMarket;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Queries.GetMarketById;
using MiKompri.ProductCatalog.Application.Queries.GetMarkets;

namespace MiKompri.ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/v1/markets")]
public class MarketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<MarketDto>> Create([FromBody] CreateMarketRequest request, CancellationToken cancellationToken)
    {
        var marketId = await _mediator.Send(new CreateMarketCommand(request.Name, request.LocationHint), cancellationToken);
        var dto = await _mediator.Send(new GetMarketByIdQuery(marketId), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { marketId }, dto);
    }

    [HttpPut("{marketId:guid}")]
    public async Task<ActionResult<MarketDto>> Update(Guid marketId, [FromBody] UpdateMarketRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateMarketCommand(marketId, request.Name, request.LocationHint), cancellationToken);
        var dto = await _mediator.Send(new GetMarketByIdQuery(marketId), cancellationToken);

        return Ok(dto);
    }

    [HttpPatch("{marketId:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid marketId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateMarketCommand(marketId), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MarketDto>>> Get(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMarketsQuery(includeInactive, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{marketId:guid}")]
    public async Task<ActionResult<MarketDto>> GetById(Guid marketId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMarketByIdQuery(marketId), cancellationToken);
        return Ok(result);
    }
}
