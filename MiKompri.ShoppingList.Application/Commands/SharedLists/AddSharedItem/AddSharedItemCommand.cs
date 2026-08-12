using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.AddSharedItem
{
    public sealed record AddSharedItemCommand(
        Guid SharedListId,
        Guid ProductId,
        string Name,
        decimal? EstimatedPrice,
        int Quantity) : IRequest<Guid>;
}
