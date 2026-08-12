using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateSharedList
{
    public sealed record UpdateSharedListCommand(Guid SharedListId, string? Name, string? Description) : IRequest;
}
