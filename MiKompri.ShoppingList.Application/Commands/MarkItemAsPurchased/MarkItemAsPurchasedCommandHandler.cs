using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased
{
    public class MarkItemAsPurchasedCommandHandler : IRequestHandler<MarkItemAsPurchasedCommand>
    {
        IPurchaseListRepository _repo;
        IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService? _user;

        public MarkItemAsPurchasedCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork, ICurrentUserService? user = null)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _user = user;
        }
        public async Task Handle(MarkItemAsPurchasedCommand request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByIdAsync(request.ListId)
                    ?? throw new KeyNotFoundException("Lista no encontrada");

            var item = list.Items.SingleOrDefault(i => i.Id == request.ItemId) ?? throw new KeyNotFoundException("El producto no está disponible.");
            if (item.SetPurchaseState(true, _user?.UserId ?? item.UpdatedBy))
            {
                list.Touch();
                if (list.IsShared) await _repo.AddSharedListAuditEventAsync(new Domain.Entities.SharedListAuditEvent(list.Id, _user?.UserId ?? item.UpdatedBy, "PurchaseStateChanged", nameof(Domain.Entities.ListItem), item.Id));
            }
            await _repo.UpdateAsync(list);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
