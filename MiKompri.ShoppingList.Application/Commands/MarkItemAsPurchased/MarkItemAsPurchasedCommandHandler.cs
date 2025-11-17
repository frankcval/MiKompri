using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased
{
    public class MarkItemAsPurchasedCommandHandler : IRequestHandler<MarkItemAsPurchasedCommand>
    {
        IPurchaseListRepository _repo;
        IUnitOfWork _unitOfWork;

        public MarkItemAsPurchasedCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(MarkItemAsPurchasedCommand request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByIdAsync(request.ListId)
                    ?? throw new KeyNotFoundException("Lista no encontrada");

            list.MarkItemAsPurchased(request.ItemId);
            await _repo.UpdateAsync(list);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
