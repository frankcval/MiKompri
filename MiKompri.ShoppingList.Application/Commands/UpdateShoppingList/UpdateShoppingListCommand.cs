using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Commands.UpdateShoppingList
{
    public record UpdateShoppingListCommand(Guid ListId, string? Name, Guid? GroupId = null) : IRequest
    {
    }
}
