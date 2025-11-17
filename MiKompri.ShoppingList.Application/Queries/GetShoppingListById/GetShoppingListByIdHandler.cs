using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.GetShoppingListById
{
    public class GetShoppingListByIdHandler : IRequestHandler<GetShoppingListByIdQuery, PurchaseListDTO>
    {
        IPurchaseListRepository _repo;

        public GetShoppingListByIdHandler(IPurchaseListRepository repo)
        {
            _repo = repo;
        }

        //TODO pasar el query a una consulat mas rapida, que no pasae por el repo
        public async Task<PurchaseListDTO> Handle(GetShoppingListByIdQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException("Lista no encontrada");

            return new PurchaseListDTO
            {
                Id = list.Id,
                Name = list.Name,
                GroupId = list.GroupId,
                CompletionPercentage = list.Progress.Percentage,
                OwnerId = list.OwnerId,
                Items = list.Items.Select(i => new ListItemDto
                {
                    Id = i.Id,
                    IsPurchased = i.IsPurchased,
                    ProducId = i.ProductId,
                    ProductName = i.Name,
                    ProductPrice = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}
