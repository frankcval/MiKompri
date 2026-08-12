using MediatR;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.CloseSharedList
{
    public sealed class CloseSharedListCommandHandler : IRequestHandler<CloseSharedListCommand>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public CloseSharedListCommandHandler(
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

        public async Task Handle(CloseSharedListCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Authentication failed.");
            }

            var list = await _repository.GetByIdAsync(request.SharedListId)
                ?? throw new KeyNotFoundException("Lista no encontrada.");

            if (!list.IsShared || !list.GroupId.HasValue)
            {
                throw new ForbiddenOperationException("La operación solo aplica a listas compartidas.");
            }

            var membership = await _groupAuthorization.GetMembershipAsync(list.GroupId.Value, _currentUser.UserId, cancellationToken);
            if (!membership.IsAuthorizedMember || membership.Role is not (GroupRole.Owner or GroupRole.Admin))
            {
                throw new ForbiddenOperationException("No tienes permisos para cerrar esta lista compartida.");
            }

            list.CloseSharedList();

            await _repository.UpdateAsync(list);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
