using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.CloseSharedList
{
    public sealed class CloseSharedListCommandValidator : AbstractValidator<CloseSharedListCommand>
    {
        public CloseSharedListCommandValidator()
        {
            RuleFor(x => x.SharedListId).NotEmpty();
        }
    }
}
