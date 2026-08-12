using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListsByGroup
{
    public sealed class GetSharedListsByGroupQueryHandler : IRequestHandler<GetSharedListsByGroupQuery, IEnumerable<PurchaseListDTO>>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public GetSharedListsByGroupQueryHandler(
            IPurchaseListRepository repository,
            ICurrentUserService currentUser,
            IGroupAuthorizationService groupAuthorization)
        {
            _repository = repository;
            _currentUser = currentUser;
            _groupAuthorization = groupAuthorization;
        }

        public async Task<IEnumerable<PurchaseListDTO>> Handle(GetSharedListsByGroupQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Authentication failed.");
            }

            var membership = await _groupAuthorization.GetMembershipAsync(request.GroupId, _currentUser.UserId, cancellationToken);
            if (!membership.IsAuthorizedMember)
            {
                throw new ForbiddenOperationException("No tienes acceso a las listas compartidas de este grupo.");
            }

            var lists = await _repository.GetByGroupAsync(request.GroupId);
            return lists.Select(x => x.ToDto());
        }
    }
}
