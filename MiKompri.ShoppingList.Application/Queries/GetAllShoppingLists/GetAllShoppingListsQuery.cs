using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Queries.GetAllShoppingLists
{
    public record GetAllShoppingListsQuery: IRequest<IEnumerable<PurchaseListDTO>>
    {
    }
}
