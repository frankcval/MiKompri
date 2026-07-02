using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.GetShoppingListByGroupId
{
    public class GetShoppingListByGroupIdHandler : IRequestHandler<GetShoppingListByGroupIdQuery, IEnumerable<PurchaseListDTO>>
    {
        IPurchaseListRepository _repo;
        public GetShoppingListByGroupIdHandler(IPurchaseListRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<PurchaseListDTO>> Handle(GetShoppingListByGroupIdQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByGroupAsync(request.GroupId)
                ?? throw new KeyNotFoundException("Lista no existe");

            return list.Select(x => x.ToDto());

        }
    }
}
