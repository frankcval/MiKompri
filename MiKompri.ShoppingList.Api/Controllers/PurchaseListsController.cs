using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiKompri.ShoppingList.Api.Models;
using MiKompri.ShoppingList.Application.Commands.CreateShoppingList;
using MiKompri.ShoppingList.Application.DTOs;
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
        /* [HttpPost]
            public async Task<ActionResult<Guid>> Create([FromBody] CreatePurchaseListRequest request)
            {
                var command = new CreateShoppingListCommand(
                    request.Name,
                    request.OwnerId,
                    request.GroupId
                );

                var id = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
        */


        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePurchaseListRequest request)
        {
            var command = new CreateShoppingListCommand(
                request.Name,
                request.OwnerId,
                request.GroupId
            );
            await _mediator.Send(command);
            var id = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }


        // GET api/v1/purchaselists/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PurchaseListDTO>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetShoppingListByIdQuery(id));
            return Ok(result);
        }


    }
}
