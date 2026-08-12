using FluentValidation;

namespace MiKompri.ShoppingList.Application.Commands.SharedLists.CreateSharedList
{
    public sealed class CreateSharedListCommandValidator : AbstractValidator<CreateSharedListCommand>
    {
        public CreateSharedListCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(120);

            RuleFor(x => x.GroupId)
                .NotEmpty();

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => x.Description is not null);
        }
    }
}
