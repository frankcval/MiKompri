using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased
{
    public record MarkItemAsPurchasedCommand(Guid ListId, Guid ItemId) : IRequest
    {
    }
}
