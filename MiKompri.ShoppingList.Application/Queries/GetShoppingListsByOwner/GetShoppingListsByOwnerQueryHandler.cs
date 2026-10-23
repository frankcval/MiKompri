using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.GetShoppingListsByOwner
{
    public class GetShoppingListsByOwnerQueryHandler : IRequestHandler<GetShoppingListsByOwnerQuery, IEnumerable<PurchaseListDTO>>
    {
        private readonly IPurchaseListRepository _repo;
        private readonly ICurrentUserService _currentUser;

        public GetShoppingListsByOwnerQueryHandler(IPurchaseListRepository repo, ICurrentUserService currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<PurchaseListDTO>> Handle(GetShoppingListsByOwnerQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Authentication failed.");
            }

            if (_currentUser.UserId != request.OwnerId)
            {
                throw new ForbiddenOperationException("No tienes acceso a las listas personales de otro usuario.");
            }

            var list = await _repo.GetByOwnerAsync(request.OwnerId)
                ?? throw new KeyNotFoundException("No hay lista de compra para este owner");

            return list.Select(lst => lst.ToDto());
        }
    }
}
