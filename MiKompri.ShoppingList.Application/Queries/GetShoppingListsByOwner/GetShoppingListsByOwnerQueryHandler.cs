using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.GetShoppingListsByOwner
{
    public class GetShoppingListsByOwnerQueryHandler : IRequestHandler<GetShoppingListsByOwnerQuery, IEnumerable<PurchaseListDTO>>
    {
        IPurchaseListRepository _repo;
        public GetShoppingListsByOwnerQueryHandler(IPurchaseListRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<PurchaseListDTO>> Handle(GetShoppingListsByOwnerQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByOwnerAsync(request.OwnerId)
                ?? throw new KeyNotFoundException("No hay lista de compra para este owner");


            return list.Select(lst => lst.ToDto());


        }
    }
}
