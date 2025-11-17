using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased
{
    public class MarkItemAsPurchasedValidator : AbstractValidator<MarkItemAsPurchasedCommand>
    {
        public MarkItemAsPurchasedValidator()
        {
            RuleFor(x => x.ListId).NotEmpty()
                 .NotNull()
                 .WithMessage("el identificador de la lista no debe estar vacio");

            RuleFor(x => x.ItemId).NotNull()
                .NotEmpty()
                .WithMessage("el identificador del item de la lista no debe estar vacio");
        }
    }
}
