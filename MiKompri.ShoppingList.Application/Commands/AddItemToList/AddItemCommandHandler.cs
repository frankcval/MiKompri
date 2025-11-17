using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.AddItemToList
{
    public class AddItemCommandHandler : IRequestHandler<AddItemCommand>
    {
        IPurchaseListRepository _repo;
        IUnitOfWork _unitOfWork;

        public AddItemCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(AddItemCommand request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByIdAsync(request.ListId)
               ?? throw new KeyNotFoundException("Lista no econtrada");

            list.AddItem(new ListItem(request.ProductId, request.Name, request.Price, request.Quantity));
            await _repo.UpdateAsync(list);
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
