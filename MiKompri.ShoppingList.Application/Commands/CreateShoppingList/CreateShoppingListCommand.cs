using MediatR;

namespace MiKompri.ShoppingList.Application.Commands.CreateShoppingList
{
    //public record CreateShoppingListCommand(string Name, Guid OwnerId, Guid? GroupId = null) : IRequest<Guid>;

    public record CreateShoppingListCommand(string Name, Guid OwnerId, Guid? GroupId = null) : IRequest<Guid>
    {
    }
}
