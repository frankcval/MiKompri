using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.RegisterItemExpense
{
    public sealed class RegisterItemExpenseCommandValidator : AbstractValidator<RegisterItemExpenseCommand>
    {
        public RegisterItemExpenseCommandValidator()
        {
            RuleFor(x => x.SharedListId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.PaidBy).NotEmpty();
            RuleFor(x => x.RealPaidPrice).GreaterThan(0);
            RuleFor(x => x.Participants).NotNull().Must(p => p.Count > 0);
            RuleFor(x => x.Participants).Must(p => p.Distinct().Count() == p.Count)
                .When(x => x.Participants is not null)
                .WithMessage("No se permiten participantes duplicados.");
        }
    }
}
