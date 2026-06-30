using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Commands.UpdateShoppingList
{
    public class UpdateShoppingListValidator : AbstractValidator<UpdateShoppingListCommand>
    {
        public UpdateShoppingListValidator()
        {
            RuleFor(x => x.ListId).NotEmpty()
            .NotNull()
            .WithMessage("el identificador de la lista no debe estar vacio");

            When(x => !string.IsNullOrWhiteSpace(x.Name),
              () => RuleFor(n => n.Name)
              .MaximumLength(100)
              .WithMessage("El nombre no puede superar los 100 caracteres")
              );

            When(x => x.GroupId.HasValue,
                () => RuleFor(n => n.GroupId)
                .NotEmpty()
                .WithMessage("El identificador del grupo no debe estar vacio")
                );

            //validando que venga al menos el nombre o el id del grupo
            RuleFor(x => new { x.Name, x.GroupId })
            .Must(x => !string.IsNullOrWhiteSpace(x.Name) || x.GroupId.HasValue)
            .WithMessage("Debe especificar al menos un campo a actualizar.");
        }
    }
}
