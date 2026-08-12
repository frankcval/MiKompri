using MediatR;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Services;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.RegisterItemExpense
{
    public sealed class RegisterItemExpenseCommandHandler : IRequestHandler<RegisterItemExpenseCommand, Guid>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;
        private readonly ActiveMembershipPolicy _activeMembershipPolicy;

        public RegisterItemExpenseCommandHandler(
            IPurchaseListRepository repository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IGroupAuthorizationService groupAuthorization,
            ActiveMembershipPolicy activeMembershipPolicy)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _groupAuthorization = groupAuthorization;
            _activeMembershipPolicy = activeMembershipPolicy;
        }

        public async Task<Guid> Handle(RegisterItemExpenseCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
                throw new UnauthorizedAccessException("Authentication failed.");

            var list = await _repository.GetByIdAsync(request.SharedListId)
                ?? throw new KeyNotFoundException("Lista no encontrada.");

            if (!list.IsShared || !list.GroupId.HasValue)
                throw new ForbiddenOperationException("La operación solo aplica a listas compartidas.");

            var membership = await _groupAuthorization.GetMembershipAsync(list.GroupId.Value, _currentUser.UserId, cancellationToken);
            if (!membership.IsAuthorizedMember)
                throw new ForbiddenOperationException("No tienes permisos para registrar gastos en esta lista compartida.");

            if (membership.Role == GroupRole.Member && request.PaidBy != _currentUser.UserId)
                throw new ForbiddenOperationException("Los miembros solo pueden registrar pagos propios.");

            await _activeMembershipPolicy.EnsureActiveMemberAsync(list.GroupId.Value, request.PaidBy, "PaidBy", cancellationToken);
            if (request.PurchasedBy.HasValue)
            {
                await _activeMembershipPolicy.EnsureActiveMemberAsync(list.GroupId.Value, request.PurchasedBy.Value, "PurchasedBy", cancellationToken);
            }
            await _activeMembershipPolicy.EnsureActiveMembersAsync(list.GroupId.Value, request.Participants, "Participants", cancellationToken);

            var item = list.Items.FirstOrDefault(x => x.Id == request.ItemId)
                ?? throw new KeyNotFoundException("Ítem no encontrado.");

            var expense = item.RegisterExpense(request.PaidBy, request.PurchasedBy, request.RealPaidPrice, request.Participants, request.Currency);

            await _repository.AddSharedListAuditEventAsync(new SharedListAuditEvent(list.Id, _currentUser.UserId, "ExpenseRecorded", nameof(ItemExpenseRecord), expense.Id));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return expense.Id;
        }
    }
}
