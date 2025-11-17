using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.AddItemToList
{
    public class AddItemCommandValidator : AbstractValidator<AddItemCommand>
    {
        public AddItemCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("EL nombre es obligatorio")
            .MaximumLength(200);

            RuleFor(x => x.Quantity).GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor que cero");

        }
    }
}
