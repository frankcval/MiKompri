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
        private readonly ICurrentUserService _user;

        public GetAllShoppingListsQueryHandler(IPurchaseListRepository repo, ICurrentUserService user)
        {
            _repo = repo;
            _user = user;
        }
        public async Task<IEnumerable<PurchaseListDTO>> Handle(GetAllShoppingListsQuery request, CancellationToken cancellationToken)
        {
            var lists = await _repo.GetByOwnerAsync(_user.UserId) ?? Enumerable.Empty<PurchaseList>();

            return lists.Where(pl => !pl.IsShared).Select(pl => pl.ToDto());
        }
    }
}
