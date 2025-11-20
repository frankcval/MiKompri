using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.CreateShoppingList
{
    public class CreateShoppingListValidator : AbstractValidator<CreateShoppingListCommand>
    {
        public CreateShoppingListValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
            .WithMessage("EL nombre es obligatorio")
            .MaximumLength(200);

            RuleFor(x => x.OwnerId)
                         .NotEmpty().WithMessage("OwnerId es obligatorio")
                         .Must(id => id != Guid.Empty).WithMessage("OwnerId no puede ser Guid.Empty");

        }
    }
}
