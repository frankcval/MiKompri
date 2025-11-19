using MediatR;
using MiKompri.ShoppingList.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Commands.DeleteShoppinList
{
    public class DeletePurchaseListCommandHandler : IRequestHandler<DeletePurchaseListCommand>
    {

        private readonly IPurchaseListRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePurchaseListCommandHandler(IPurchaseListRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(DeletePurchaseListCommand request, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(request.ListId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
