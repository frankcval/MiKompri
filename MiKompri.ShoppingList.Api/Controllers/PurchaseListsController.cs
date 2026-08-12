using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiKompri.ShoppingList.Api.Models;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Application.Commands.AddItemToList;
using MiKompri.ShoppingList.Application.Commands.CreateShoppingList;
using MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList;
using MiKompri.ShoppingList.Application.Commands.DeleteShoppinList;
using MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased;
using MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList;
using MiKompri.ShoppingList.Application.Commands.UpdateShoppingList;
using MiKompri.ShoppingList.Application.Commands.SharedLists.AddSharedItem;
using MiKompri.ShoppingList.Application.Commands.SharedLists.CloseSharedList;
using MiKompri.ShoppingList.Application.Commands.SharedLists.CreateSharedList;
using MiKompri.ShoppingList.Application.Commands.SharedLists.DeleteItemExpense;
using MiKompri.ShoppingList.Application.Commands.SharedLists.RegisterItemExpense;
using MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateItemExpense;
using MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateSharedList;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Queries.GetAllShoppingLists;
using MiKompri.ShoppingList.Application.Queries.GetItemListById;
using MiKompri.ShoppingList.Application.Queries.GetShoppingListByGroupId;
using MiKompri.ShoppingList.Application.Queries.GetShoppingListById;
using MiKompri.ShoppingList.Application.Queries.GetShoppingListsByOwner;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListAuditEvents;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListById;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListsByGroup;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementProposal;
using MiKompri.ShoppingList.Application.Queries.SharedLists.GetSettlementSummary;

namespace MiKompri.ShoppingList.Api.Controllers
{

    [ApiController]
    [Route("api/v1/[controller]")]
    public class PurchaseListsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseListsController(IMediator mediator)
        {
            _mediator = mediator;
        }
     
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePurchaseListRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateShoppingListCommand(
                request.Name,
                request.OwnerId,
                request.GroupId
            );

            var id = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [Authorize]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPost("/api/v1/shared-lists")]
        public async Task<ActionResult<Guid>> CreateSharedList([FromBody] CreateSharedListRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateSharedListCommand(request.Name, request.GroupId, request.Description);
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetSharedListById), new { sharedListId = id }, id);
        }

        [Authorize]
        [ProducesResponseType(typeof(PurchaseListDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("/api/v1/shared-lists/{sharedListId:guid}")]
        public async Task<ActionResult<PurchaseListDTO>> GetSharedListById(Guid sharedListId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSharedListByIdQuery(sharedListId), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PurchaseListDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("/api/v1/shared-lists")]
        public async Task<ActionResult<IEnumerable<PurchaseListDTO>>> GetSharedListsByGroup([FromQuery] Guid groupId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSharedListsByGroupQuery(groupId), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPatch("/api/v1/shared-lists/{sharedListId:guid}")]
        public async Task<IActionResult> UpdateSharedList(Guid sharedListId, [FromBody] UpdateSharedListRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(new UpdateSharedListCommand(sharedListId, request.Name, request.Description), cancellationToken);
            return NoContent();
        }

        [Authorize]
        [HttpPatch("/api/v1/shared-lists/{sharedListId:guid}/close")]
        public async Task<IActionResult> CloseSharedList(Guid sharedListId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CloseSharedListCommand(sharedListId), cancellationToken);
            return NoContent();
        }

        [Authorize]
        [ProducesResponseType(typeof(SettlementSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("/api/v1/shared-lists/{sharedListId:guid}/settlement/summary")]
        public async Task<ActionResult<SettlementSummaryDto>> GetSettlementSummary(Guid sharedListId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSettlementSummaryQuery(sharedListId), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [ProducesResponseType(typeof(SettlementProposalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("/api/v1/shared-lists/{sharedListId:guid}/settlement/proposal")]
        public async Task<ActionResult<SettlementProposalDto>> GetSettlementProposal(Guid sharedListId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSettlementProposalQuery(sharedListId), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<SharedListAuditEventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("/api/v1/shared-lists/{sharedListId:guid}/audit-events")]
        public async Task<ActionResult<IEnumerable<SharedListAuditEventDto>>> GetAuditEvents(Guid sharedListId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSharedListAuditEventsQuery(sharedListId), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPost("/api/v1/shared-lists/{sharedListId:guid}/items")]
        public async Task<ActionResult<Guid>> AddSharedItem(Guid sharedListId, [FromBody] AddSharedItemRequest request, CancellationToken cancellationToken)
        {
            var command = new AddSharedItemCommand(sharedListId, request.ProductId, request.Name, request.EstimatedPrice, request.Quantity);
            var id = await _mediator.Send(command, cancellationToken);
            return Created($"/api/v1/shared-lists/{sharedListId}/items/{id}", id);
        }

        [Authorize]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPost("/api/v1/shared-lists/{sharedListId:guid}/items/{itemId:guid}/expenses")]
        public async Task<ActionResult<Guid>> RegisterItemExpense(Guid sharedListId, Guid itemId, [FromBody] RegisterExpenseRequest request, CancellationToken cancellationToken)
        {
            var command = new RegisterItemExpenseCommand(
                sharedListId,
                itemId,
                request.PaidBy,
                request.PurchasedBy,
                request.RealPaidPrice,
                request.Currency,
                request.Participants);

            var expenseId = await _mediator.Send(command, cancellationToken);
            return Created($"/api/v1/shared-lists/{sharedListId}/items/{itemId}/expenses/{expenseId}", expenseId);
        }

        [Authorize]
        [HttpPatch("/api/v1/shared-lists/{sharedListId:guid}/items/{itemId:guid}/expenses/{expenseId:guid}")]
        public async Task<IActionResult> UpdateItemExpense(Guid sharedListId, Guid itemId, Guid expenseId, [FromBody] UpdateExpenseRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateItemExpenseCommand(
                sharedListId,
                itemId,
                expenseId,
                request.PaidBy,
                request.PurchasedBy,
                request.RealPaidPrice,
                request.Currency,
                request.Participants);

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("/api/v1/shared-lists/{sharedListId:guid}/items/{itemId:guid}/expenses/{expenseId:guid}")]
        public async Task<IActionResult> DeleteItemExpense(Guid sharedListId, Guid itemId, Guid expenseId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteItemExpenseCommand(sharedListId, itemId, expenseId), cancellationToken);
            return NoContent();
        }

        //GET obtener lista de compras
        [HttpGet]
        public async Task<ActionResult<List<PurchaseListDTO>>> GetAll(
                [FromQuery] Guid? ownerId,
                [FromQuery] Guid? groupId, 
                CancellationToken cancellationToken)
        {

            if (ownerId.HasValue)
            {
                var result = await _mediator.Send(new GetShoppingListsByOwnerQuery(ownerId.Value), cancellationToken);
                return Ok(result);
            }

            if (groupId.HasValue)
            {
                var result = await _mediator.Send(new GetShoppingListByGroupIdQuery(groupId.Value), cancellationToken);
                return Ok(result);
            }
            var all = await _mediator.Send(new GetAllShoppingListsQuery(), cancellationToken);
            return Ok(all);
        }

        // GET api/v1/purchaselists/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PurchaseListDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetShoppingListByIdQuery(id), cancellationToken);
            return Ok(result);
        }


        //HttpDelete api/v1/purchaselists/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeletePurchaseListCommand(id), cancellationToken);
            return NoContent();
        }

        //update lista de compras
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseListRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateShoppingListCommand(
                id,
                request.Name,
                request.GroupId
            );
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        // Items de la lista de Compras
        //Crear item de lista de compra
        [HttpPost("{listId:guid}/items")]
        public async Task<IActionResult> AddItem(Guid listId, [FromBody] AddItemRequest request, CancellationToken cancellationToken)
        {
            var command = new AddItemCommand(
                listId,
                request.ProductId,
                request.ProductName,
                request.Price,
                request.Quantity
            );
            var id =  await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetItemById), new { listId = listId, itemId = id }, id);  //se debe retornar el id del item creado, para ello debe corresponder con la llamada al metod getitemlist, son dos parametros


        }

        //Actualizar item de lista de compra
        [HttpPut("{listId:guid}/items/{itemId:guid}")]
        public async Task<IActionResult> UpdateItem(Guid listId, Guid itemId, [FromBody] UpdateItemRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateItemShoopingListCommand(
                listId,
                itemId,
                request.ProductName,
                request.Price,
                request.Quantity
            );
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        //Eliminar item de lista de compra
        [HttpDelete("{listId:guid}/items/{itemId:guid}")]
        public async Task<IActionResult> DeleteItem(Guid listId, Guid itemId, CancellationToken cancellationToken)
        {
            var command = new DeleteItemShoppingListCommand(listId, itemId);
            var result = await _mediator.Send(command, cancellationToken);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        //marcar item como comprado
        [HttpPost("{listId:guid}/items/{itemId:guid}/mark-as-purchased")]
        public async Task<IActionResult> MarkItemAsPurchased(Guid listId, Guid itemId, CancellationToken cancellationToken)
        {
            var command = new MarkItemAsPurchasedCommand(listId, itemId);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }


        //obtener el item de la lista de compras
        [HttpGet("{listId:guid}/items/{itemId:guid}")]
        public async Task<IActionResult> GetItemById(Guid listId, Guid itemId, CancellationToken cancellationToken)
        {
            //Implementar este método usando MediatR para enviar una consulta que obtenga el ítem por su ID.
            var command = new GetItemListByIdQuery(listId, itemId);
            var item =  await _mediator.Send(command, cancellationToken);
            //retornar el item obtenido
            return Ok(item);
        }
    }
}

