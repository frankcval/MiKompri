using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListById
{
    public sealed class GetSharedListByIdQueryHandler : IRequestHandler<GetSharedListByIdQuery, PurchaseListDTO>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public GetSharedListByIdQueryHandler(
            IPurchaseListRepository repository,
            ICurrentUserService currentUser,
            IGroupAuthorizationService groupAuthorization)
        {
            _repository = repository;
            _currentUser = currentUser;
            _groupAuthorization = groupAuthorization;
        }

        public async Task<PurchaseListDTO> Handle(GetSharedListByIdQuery request, CancellationToken cancellationToken)
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
            if (!membership.IsAuthorizedMember)
            {
                throw new ForbiddenOperationException("No tienes acceso a esta lista compartida.");
            }

            return list.ToDto();
        }
    }
}
