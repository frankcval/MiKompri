using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList
{
    public class DeleteItemShoppingListCommandHandler : IRequestHandler<DeleteItemShoppingListCommand, bool>
    {
        IPurchaseListRepository _repo;
        IUnitOfWork _unitOfWork;

        public DeleteItemShoppingListCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteItemShoppingListCommand request, CancellationToken cancellationToken)
        {
            await _repo.DeleteItemAsync(request.ListId, request.ProductId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
