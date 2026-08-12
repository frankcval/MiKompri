using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.AddSharedItem
{
    public sealed class AddSharedItemCommandValidator : AbstractValidator<AddSharedItemCommand>
    {
        public AddSharedItemCommandValidator()
        {
            RuleFor(x => x.SharedListId).NotEmpty();
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.EstimatedPrice).GreaterThanOrEqualTo(0).When(x => x.EstimatedPrice.HasValue);
        }
    }
}
