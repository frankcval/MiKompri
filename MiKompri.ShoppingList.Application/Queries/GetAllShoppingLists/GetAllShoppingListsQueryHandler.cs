using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Queries.GetAllShoppingLists
{
    public class GetAllShoppingListsQueryHandler : IRequestHandler<GetAllShoppingListsQuery, IEnumerable<PurchaseListDTO>>
    {
        IPurchaseListRepository _repo;

        public GetAllShoppingListsQueryHandler(IPurchaseListRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<PurchaseListDTO>> Handle(GetAllShoppingListsQuery request, CancellationToken cancellationToken)
        {
            var lists = await _repo.GetAllAsync() ?? Enumerable.Empty<PurchaseList>(); ;
              //  ?? Task.FromResult<IEnumerable<Domain.Entities.PurchaseList>>(new List<Domain.Entities.PurchaseList>());    
                
                return lists.Select(pl => new PurchaseListDTO
                    {
                        Id = pl.Id,
                        Name = pl.Name,
                        OwnerId = pl.OwnerId,
                        GroupId = pl.GroupId,
                        CompletionPercentage = pl.Progress.Percentage,
                        Items = pl.Items.Select(li => new ListItemDto
                        {
                            Id = li.Id,
                            ProductName = li.Name,
                            IsPurchased = li.IsPurchased,
                            Quantity = li.Quantity,
                            ProducId = li.ProductId,
                            ProductPrice = li.Price

                        }).ToList()
                    });
        }
    }
}
