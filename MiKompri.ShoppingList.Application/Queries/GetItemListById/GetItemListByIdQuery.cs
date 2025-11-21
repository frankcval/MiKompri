using MediatR;
using MiKompri.ShoppingList.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Queries.GetItemListById
{
    public record GetItemListByIdQuery(Guid IdList, Guid IdItem) : IRequest<ListItemDto>
    {
    }
}
