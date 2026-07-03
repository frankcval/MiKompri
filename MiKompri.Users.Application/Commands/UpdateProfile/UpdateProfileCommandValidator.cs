using FluentValidation;

namespace MiKompri.Users.Application.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("El nombre visible es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre visible no puede superar los 100 caracteres.");
        }
    }
}
