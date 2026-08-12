using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.CreateSharedList
{
    public sealed record CreateSharedListCommand(string Name, Guid GroupId, string? Description) : IRequest<Guid>;
}
