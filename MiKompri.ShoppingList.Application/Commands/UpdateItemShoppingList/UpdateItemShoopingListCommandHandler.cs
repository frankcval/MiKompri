using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList
{
    public class UpdateItemShoopingListCommandHandler : IRequestHandler<UpdateItemShoopingListCommand, bool>
    {
        IPurchaseListRepository _repoOperational;
        IUnitOfWork _unitOfWork;

        public UpdateItemShoopingListCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork)
        {
            _repoOperational = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateItemShoopingListCommand request, CancellationToken cancellationToken)
        {
            await _repoOperational.UpdateItemAsync(request.listId, request.ProdId, request.ProductName, request.Price.Value, request.Quantity.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
