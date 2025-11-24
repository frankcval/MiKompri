using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList
{
    public class DeleteItemShoppingListValidator : AbstractValidator<DeleteItemShoppingListCommand>
    {
        public DeleteItemShoppingListValidator()
        {
            RuleFor(x => x.ListId).NotEmpty()
                  .NotNull()
                  .WithMessage("el identificador de la lista no debe estar vacio");

            RuleFor(x => x.ProductId).NotNull()
                .NotEmpty()
                .WithMessage("el identificador del producto de la lista no debe estar vacio");
        }
    }
}
