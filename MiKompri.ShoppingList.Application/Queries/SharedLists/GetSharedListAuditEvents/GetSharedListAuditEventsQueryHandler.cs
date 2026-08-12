using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetSharedListAuditEvents
{
    public sealed class GetSharedListAuditEventsQueryHandler : IRequestHandler<GetSharedListAuditEventsQuery, IEnumerable<SharedListAuditEventDto>>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public GetSharedListAuditEventsQueryHandler(
            IPurchaseListRepository repository,
            ICurrentUserService currentUser,
            IGroupAuthorizationService groupAuthorization)
        {
            _repository = repository;
            _currentUser = currentUser;
            _groupAuthorization = groupAuthorization;
        }

        public async Task<IEnumerable<SharedListAuditEventDto>> Handle(GetSharedListAuditEventsQuery request, CancellationToken cancellationToken)
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
                throw new ForbiddenOperationException("No tienes acceso al historial de esta lista compartida.");
            }

            var events = await _repository.GetSharedListAuditEventsAsync(list.Id, cancellationToken);
            return events.Select(x => new SharedListAuditEventDto
            {
                Id = x.Id,
                SharedPurchaseListId = x.SharedPurchaseListId,
                ActorUserId = x.ActorUserId,
                ActionType = x.ActionType,
                TargetEntityType = x.TargetEntityType,
                TargetEntityId = x.TargetEntityId,
                OccurredAt = x.OccurredAt,
                Metadata = x.Metadata
            });
        }
    }
}
