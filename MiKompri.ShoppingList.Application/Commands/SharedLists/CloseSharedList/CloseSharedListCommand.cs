using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.CloseSharedList
{
    public sealed record CloseSharedListCommand(Guid SharedListId) : IRequest;
}
