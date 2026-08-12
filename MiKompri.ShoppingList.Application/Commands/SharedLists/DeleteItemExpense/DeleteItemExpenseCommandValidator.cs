using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.DeleteItemExpense
{
    public sealed class DeleteItemExpenseCommandValidator : AbstractValidator<DeleteItemExpenseCommand>
    {
        public DeleteItemExpenseCommandValidator()
        {
            RuleFor(x => x.SharedListId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.ExpenseId).NotEmpty();
        }
    }
}
