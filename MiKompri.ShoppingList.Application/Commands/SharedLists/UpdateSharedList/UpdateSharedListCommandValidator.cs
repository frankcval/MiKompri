using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateSharedList
{
    public sealed class UpdateSharedListCommandValidator : AbstractValidator<UpdateSharedListCommand>
    {
        public UpdateSharedListCommandValidator()
        {
            RuleFor(x => x.SharedListId).NotEmpty();
            RuleFor(x => x.Name).MaximumLength(120).When(x => x.Name is not null);
            RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        }
    }
}
