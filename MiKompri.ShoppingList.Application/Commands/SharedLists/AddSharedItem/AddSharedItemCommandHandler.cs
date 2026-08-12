using MediatR;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Services;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.AddSharedItem
{
    public sealed class AddSharedItemCommandHandler : IRequestHandler<AddSharedItemCommand, Guid>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;
        private readonly ActiveMembershipPolicy _activeMembershipPolicy;

        public AddSharedItemCommandHandler(
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

        public async Task<Guid> Handle(AddSharedItemCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
                throw new UnauthorizedAccessException("Authentication failed.");

            var list = await _repository.GetByIdAsync(request.SharedListId)
                ?? throw new KeyNotFoundException("Lista no encontrada.");

            if (!list.IsShared || !list.GroupId.HasValue)
                throw new ForbiddenOperationException("La operación solo aplica a listas compartidas.");

            var membership = await _groupAuthorization.GetMembershipAsync(list.GroupId.Value, _currentUser.UserId, cancellationToken);
            if (!membership.IsAuthorizedMember)
                throw new ForbiddenOperationException("No tienes permisos para agregar ítems en esta lista compartida.");

            await _activeMembershipPolicy.EnsureActiveMemberAsync(list.GroupId.Value, _currentUser.UserId, "AddedBy", cancellationToken);

            var item = new ListItem(request.ProductId, request.Name, request.EstimatedPrice ?? 0m, request.Quantity, _currentUser.UserId);
            list.AddItem(item);

            await _repository.AddSharedListAuditEventAsync(new SharedListAuditEvent(list.Id, _currentUser.UserId, "ItemAdded", nameof(ListItem), item.Id));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return item.Id;
        }
    }
}
