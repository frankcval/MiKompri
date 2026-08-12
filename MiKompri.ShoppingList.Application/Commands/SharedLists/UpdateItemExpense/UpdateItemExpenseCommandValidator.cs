using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateItemExpense
{
    public sealed class UpdateItemExpenseCommandValidator : AbstractValidator<UpdateItemExpenseCommand>
    {
        public UpdateItemExpenseCommandValidator()
        {
            RuleFor(x => x.SharedListId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.ExpenseId).NotEmpty();
            RuleFor(x => x.PaidBy).NotEmpty();
            RuleFor(x => x.RealPaidPrice).GreaterThan(0);
            RuleFor(x => x.Participants).NotNull().Must(p => p.Count > 0);
            RuleFor(x => x.Participants).Must(p => p.Distinct().Count() == p.Count)
                .When(x => x.Participants is not null)
                .WithMessage("No se permiten participantes duplicados.");
        }
    }
}
