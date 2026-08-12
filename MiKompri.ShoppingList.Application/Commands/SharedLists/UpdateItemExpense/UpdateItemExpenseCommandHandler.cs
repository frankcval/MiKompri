using MediatR;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateItemExpense
{
    public sealed class UpdateItemExpenseCommandHandler : IRequestHandler<UpdateItemExpenseCommand>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public UpdateItemExpenseCommandHandler(
            IPurchaseListRepository repository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IGroupAuthorizationService groupAuthorization)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _groupAuthorization = groupAuthorization;
        }

        public async Task Handle(UpdateItemExpenseCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
                throw new UnauthorizedAccessException("Authentication failed.");

            var list = await _repository.GetByIdAsync(request.SharedListId)
                ?? throw new KeyNotFoundException("Lista no encontrada.");

            if (!list.IsShared || !list.GroupId.HasValue)
                throw new ForbiddenOperationException("La operación solo aplica a listas compartidas.");

            var membership = await _groupAuthorization.GetMembershipAsync(list.GroupId.Value, _currentUser.UserId, cancellationToken);
            if (!membership.IsAuthorizedMember || membership.Role is not (GroupRole.Owner or GroupRole.Admin))
                throw new ForbiddenOperationException("No tienes permisos para actualizar gastos en esta lista compartida.");

            var item = list.Items.FirstOrDefault(x => x.Id == request.ItemId)
                ?? throw new KeyNotFoundException("Ítem no encontrado.");

            item.UpdateExpense(request.ExpenseId, request.PaidBy, request.PurchasedBy, request.RealPaidPrice, request.Participants, request.Currency);

            await _repository.AddSharedListAuditEventAsync(new SharedListAuditEvent(list.Id, _currentUser.UserId, "ExpenseUpdated", nameof(ItemExpenseRecord), request.ExpenseId));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
