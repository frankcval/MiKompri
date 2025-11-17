using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList
{
    public class UpdateItemShoopingListCommandHandler : IRequestHandler<UpdateItemShoopingListCommand, bool>
    {
        IPurchaseListRepository _repoOperational;

        public UpdateItemShoopingListCommandHandler(IPurchaseListRepository repo)
        {
            _repoOperational = repo;
        }

        public async Task<bool> Handle(UpdateItemShoopingListCommand request, CancellationToken cancellationToken)
        {
            await _repoOperational.UpdateItemAsync(request.listId, request.ProdId, request.ProductName, request.Price.Value, request.Quantity.Value);
            return true;
        }
    }
}
