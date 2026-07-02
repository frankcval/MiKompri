using FluentValidation;

namespace MiKompri.Users.Application.Commands.RemoveMemberFromGroup
{
    public class RemoveMemberFromGroupCommandValidator
        : AbstractValidator<RemoveMemberFromGroupCommand>
    {
        public RemoveMemberFromGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty().WithMessage("El Id del grupo es obligatorio.");

            RuleFor(x => x.TargetUserId)
                .NotEmpty().WithMessage("El Id del usuario es obligatorio.");
        }
    }
}
