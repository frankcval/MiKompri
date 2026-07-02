using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.AddItemToList
{
    public class AddItemCommandValidator : AbstractValidator<AddItemCommand>
    {
        public AddItemCommandValidator()
        {
            RuleFor(x => x.ListId)
                .NotEmpty().WithMessage("ListId es obligatorio");

            //Regla para ProductId
            RuleFor(x => x.ProductId)
                 .NotEmpty().WithMessage("ProductId es obligatorio")
                 .Must(id => id != Guid.Empty).WithMessage("ProductId no puede ser Guid.Empty");

            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("EL nombre es obligatorio")
            .MaximumLength(200);

            RuleFor(x => x.Quantity).GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor que cero");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue)
                .WithMessage("El precio no puede ser negativo");

        }
    }
}
