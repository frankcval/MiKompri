using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiKompri.ShoppingList.Application.Commands.PersonalPurchases;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetExpenses;

namespace MiKompri.ShoppingList.Api.Controllers;

[ApiController, Authorize, Route("api/v1/purchaselists/{listId:guid}")]
public sealed class PersonalPurchasesController(IMediator mediator) : ControllerBase
{
    public sealed record PurchaseInput(Guid OperationId, decimal RealPaidPrice, string Currency = "EUR", Guid? MarketId = null, DateOnly? PurchasedOn = null);
    public sealed record CorrectionInput(Guid ExpectedVersion, decimal RealPaidPrice, string Currency = "EUR", Guid? MarketId = null, DateOnly? PurchasedOn = null);

    [HttpGet("purchases")]
    public async Task<ActionResult<IReadOnlyList<ExpenseView>>> Get(Guid listId, CancellationToken ct)
        => Ok(await mediator.Send(new GetExpensesQuery(listId, true), ct));

    [HttpPost("items/{itemId:guid}/purchases")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create(Guid listId, Guid itemId, PurchaseInput input, CancellationToken ct)
    {
        var id = await mediator.Send(new RegisterPersonalPurchase(listId, itemId, input.OperationId, input.RealPaidPrice, input.Currency, input.MarketId, input.PurchasedOn), ct);
        return Created($"/api/v1/purchaselists/{listId}/purchases", id);
    }

    [HttpPatch("items/{itemId:guid}/purchases/{expenseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid listId, Guid itemId, Guid expenseId, CorrectionInput input, CancellationToken ct)
    {
        await mediator.Send(new UpdatePersonalPurchase(listId, itemId, expenseId, input.ExpectedVersion, input.RealPaidPrice, input.Currency, input.MarketId, input.PurchasedOn), ct);
        return NoContent();
    }

    [HttpDelete("items/{itemId:guid}/purchases/{expenseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid listId, Guid itemId, Guid expenseId, CancellationToken ct)
    {
        await mediator.Send(new DeletePersonalPurchase(listId, itemId, expenseId), ct);
        return NoContent();
    }
}
