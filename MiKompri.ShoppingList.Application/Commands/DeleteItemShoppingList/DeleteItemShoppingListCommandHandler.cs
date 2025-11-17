using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList
{
    public class DeleteItemShoppingListCommandHandler : IRequestHandler<DeleteItemShoppingListCommand, bool>
    {
        IPurchaseListRepository _repo;

        public DeleteItemShoppingListCommandHandler(IPurchaseListRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(DeleteItemShoppingListCommand request, CancellationToken cancellationToken)
        {
            await _repo.DeleteItemAsync(request.ListId, request.ItemId);
            return true;
        }
    }
}
