using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList
{
    public class UpdateShoppingListValidator : AbstractValidator<UpdateItemShoopingListCommand>
    {
        public UpdateShoppingListValidator()
        {
            RuleFor(x => x.listId).NotEmpty()
             .NotNull()
             .WithMessage("el identificador de la lista no debe estar vacio");

            RuleFor(x => x.ProdId).NotNull()
                .NotEmpty()
                .WithMessage("el identificador del item de la lista no debe estar vacio");


            When(x => !string.IsNullOrWhiteSpace(x.ProductName),
                () => RuleFor(n => n.ProductName)
                .MaximumLength(100)
                .WithMessage("El nombre no puede superar los 100 caracteres")
                );

            When(x => x.Price.HasValue,
                () => RuleFor(p => p.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El valor debe ser igual o mayor que cero"));

            // Validar cantidad solo si viene con valor
            When(x => x.Quantity.HasValue, () =>
            {
                RuleFor(x => x.Quantity.Value)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
            });

            RuleFor(x => new { x.ProductName, x.Price, x.Quantity })
                .Must(x => !string.IsNullOrWhiteSpace(x.ProductName) || x.Price.HasValue || x.Quantity.HasValue)
                .WithMessage("Debe especificar al menos un campo a actualizar.");


        }
    }
}
