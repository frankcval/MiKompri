using MediatR;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.CreateSharedList
{
    public sealed class CreateSharedListCommandHandler : IRequestHandler<CreateSharedListCommand, Guid>
    {
        private readonly IPurchaseListRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public CreateSharedListCommandHandler(
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

        public async Task<Guid> Handle(CreateSharedListCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Authentication failed.");
            }

            var membership = await _groupAuthorization.GetMembershipAsync(request.GroupId, _currentUser.UserId, cancellationToken);
            if (!membership.IsAuthorizedMember)
            {
                throw new ForbiddenOperationException("No tienes permisos para crear listas compartidas en este grupo.");
            }

            var list = new PurchaseList(request.Name, _currentUser.UserId, request.GroupId);
            list.UpdateDescription(request.Description);

            await _repository.AddAsync(list);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return list.Id;
        }
    }
}
