using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Commands.UpdateShoppingList
{

    public class UpdateShoppingListCommandHandler : IRequestHandler<UpdateShoppingListCommand>
    {
        private readonly IPurchaseListRepository _repo;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShoppingListCommandHandler(IPurchaseListRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(UpdateShoppingListCommand request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByIdAsync(request.ListId) 
                ?? throw new KeyNotFoundException("Lista no existe");
            if (request.Name.Length > 0) 
                list.Rename(request.Name);
            
            if (request.GroupId is not null)
                list.ChangeGroup(request.GroupId);

            await _repo.UpdateAsync(list);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        }
    }
}
