using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Commands.DeleteShoppinList
{
    public record DeletePurchaseListCommand(Guid ListId) : IRequest;
}
