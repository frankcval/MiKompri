using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiKompri.ShoppingList.Api.Models;
using MiKompri.ShoppingList.Application.Commands.AddItemToList;
using MiKompri.ShoppingList.Application.Commands.CreateShoppingList;
using MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList;
using MiKompri.ShoppingList.Application.Commands.DeleteShoppinList;
using MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased;
using MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList;
using MiKompri.ShoppingList.Application.Commands.UpdateShoppingList;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Queries.GetAllShoppingLists;
using MiKompri.ShoppingList.Application.Queries.GetItemListById;
using MiKompri.ShoppingList.Application.Queries.GetShoppingListById;

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

        //GET obtener lista de compras
        [HttpGet]
        public async Task<ActionResult<List<PurchaseListDTO>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllShoppingListsQuery(), cancellationToken);
            return Ok(result);
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
