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
        }
    }
}
