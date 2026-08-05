using FluentValidation;

namespace MiKompri.ProductCatalog.Application.Commands.CreateMarket;

public class CreateMarketCommandValidator : AbstractValidator<CreateMarketCommand>
{
    public CreateMarketCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del mercado es obligatorio")
            .MaximumLength(120);

        RuleFor(x => x.LocationHint)
            .MaximumLength(180)
            .When(x => !string.IsNullOrWhiteSpace(x.LocationHint));
    }
}
