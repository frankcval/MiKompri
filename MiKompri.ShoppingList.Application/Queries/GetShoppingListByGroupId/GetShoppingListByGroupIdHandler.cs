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
            var list = await _repo.GetByOwnerAsync(request.GroupId)
                ?? throw new KeyNotFoundException("Lista no existe");

            return list.Select(x => new PurchaseListDTO
            {
                Id = x.Id,
                Name = x.Name,
                GroupId = x.GroupId,
                OwnerId = x.OwnerId,
                CompletionPercentage = x.Progress.Percentage,
                Items = x.Items.Select(i => new ListItemDto
                {
                    Id = i.Id,
                    IsPurchased = i.IsPurchased,
                    ProducId = i.ProductId,
                    ProductName = i.Name,
                    ProductPrice = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            });

        }
    }
}
