using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.CreateShoppingList
{
    public class CreateShoppingListCommandHandler : IRequestHandler<CreateShoppingListCommand, Guid>
    {
        private readonly IPurchaseListRepository _repo;
        private readonly IUnitOfWork _unitOfWork;

        public CreateShoppingListCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateShoppingListCommand request, CancellationToken cancellationToken)
        {
            var list = new PurchaseList(request.Name, request.OwnerId, request.GroupId);
            await _repo.AddAsync(list);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return list.Id;
        }
    }
}
