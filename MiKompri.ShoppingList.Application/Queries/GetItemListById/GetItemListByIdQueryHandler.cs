using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Queries.GetItemListById
{
   
    public class GetItemListByIdQueryHandler : IRequestHandler<GetItemListByIdQuery, ListItemDto>
    {
        IPurchaseListRepository _repo;

        public GetItemListByIdQueryHandler(IPurchaseListRepository repo)
        {
            _repo = repo;
        }

        public async Task<ListItemDto> Handle(GetItemListByIdQuery request, CancellationToken cancellationToken)
        {
            //var list = await _repo.GetByIdAsync(request.IdList)
            //   ?? throw new KeyNotFoundException("Lista no encontrada");

            //var item = list.Items.FirstOrDefault(i => i.Id == request.IdItem)
            //    ?? throw new KeyNotFoundException("Item no encontrado en la lista");

            var item = await _repo.GetItemAsync(request.IdList, request.IdItem, cancellationToken)
                ?? throw new KeyNotFoundException("Item no encontrado en la lista"); //optimizado

            return new ListItemDto()
            {
                Id = item.Id,
                IsPurchased = item.IsPurchased,
                ProducId = item.ProductId,
                ProductName = item.Name,
                ProductPrice = item.Price,
                Quantity = item.Quantity
            };

        }
    }
}
