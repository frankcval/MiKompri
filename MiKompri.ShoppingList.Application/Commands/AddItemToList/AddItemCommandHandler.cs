using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.AddItemToList
{
    public class AddItemCommandHandler : IRequestHandler<AddItemCommand, Guid>
    {
        IPurchaseListRepository _repo;
        IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService? _user;

        public AddItemCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork, ICurrentUserService? user = null)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _user = user;
        }

        public async Task<Guid> Handle(AddItemCommand request, CancellationToken cancellationToken)
        {

            var item = new ListItem(
            request.ProductId,
            request.Name,
            request.Price ?? 0,
            request.Quantity,
            _user?.UserId ?? Guid.Empty
        );
            item.SetEstimatedPrice(request.Price, _user?.UserId ?? Guid.Empty);
            await _repo.AddItemAsync(request.ListId, item, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return item.Id;   // este es el Guid que devuelves en el Created

        }
    }
}
