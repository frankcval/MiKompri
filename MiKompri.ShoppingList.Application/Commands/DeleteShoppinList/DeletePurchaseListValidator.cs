using FluentValidation;
using MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Commands.DeleteShoppinList
{
    public class DeletePurchaseListValidator : AbstractValidator<DeletePurchaseListCommand>
    {
        public DeletePurchaseListValidator()
        {
            RuleFor(x => x.ListId).NotEmpty()
                  .NotNull()
                  .WithMessage("el identificador de la lista no debe estar vacio");

            //regla para un Guid valido
            RuleFor(x => x.ListId)
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("el identificador de la lista no es valido");
        }
    }
}
