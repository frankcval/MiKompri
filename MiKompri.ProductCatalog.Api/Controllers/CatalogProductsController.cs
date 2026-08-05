using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiKompri.ProductCatalog.Api.Models;
using MiKompri.ProductCatalog.Application.Commands.CreateCatalogProduct;
using MiKompri.ProductCatalog.Application.Commands.DeactivateCatalogProduct;
using MiKompri.ProductCatalog.Application.Commands.UpdateCatalogProduct;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Queries.GetCatalogProductById;
using MiKompri.ProductCatalog.Application.Queries.GetCatalogProducts;
using MiKompri.ProductCatalog.Application.Queries.GetProductPriceHistory;

namespace MiKompri.ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/v1/catalog-products")]
public class CatalogProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CatalogProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CatalogProductDto>> Create(
        [FromBody] CreateCatalogProductRequest request,
        CancellationToken cancellationToken)
    {
        var catalogProductId = await _mediator.Send(
            new CreateCatalogProductCommand(request.Name, request.PurchaseUnit),
            cancellationToken);

        var dto = await _mediator.Send(new GetCatalogProductByIdQuery(catalogProductId), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { catalogProductId }, dto);
    }

    [HttpPut("{catalogProductId:guid}")]
    public async Task<ActionResult<CatalogProductDto>> Update(
        Guid catalogProductId,
        [FromBody] UpdateCatalogProductRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateCatalogProductCommand(catalogProductId, request.Name, request.PurchaseUnit),
            cancellationToken);

        var dto = await _mediator.Send(new GetCatalogProductByIdQuery(catalogProductId), cancellationToken);
        return Ok(dto);
    }

    [HttpPatch("{catalogProductId:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid catalogProductId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateCatalogProductCommand(catalogProductId), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CatalogProductDto>>> Get(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCatalogProductsQuery(includeInactive, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{catalogProductId:guid}")]
    public async Task<ActionResult<CatalogProductDto>> GetById(Guid catalogProductId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCatalogProductByIdQuery(catalogProductId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{catalogProductId:guid}/price-history")]
    public async Task<ActionResult<ProductPriceHistoryDto>> GetPriceHistory(
        Guid catalogProductId,
        [FromQuery] Guid? marketId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProductPriceHistoryQuery(catalogProductId, marketId, from, to),
            cancellationToken);

        return Ok(result);
    }
}
