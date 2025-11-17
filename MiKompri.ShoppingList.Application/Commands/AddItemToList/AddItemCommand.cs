using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.AddItemToList
{
    public record AddItemCommand(Guid ListId, Guid ProductId, string Name, decimal Price, int Quantity) : IRequest
    {
    }
}
