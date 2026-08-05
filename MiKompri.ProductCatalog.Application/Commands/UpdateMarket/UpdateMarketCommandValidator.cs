using FluentValidation;

namespace MiKompri.ProductCatalog.Application.Commands.UpdateMarket;

public class UpdateMarketCommandValidator : AbstractValidator<UpdateMarketCommand>
{
    public UpdateMarketCommandValidator()
    {
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("marketId es obligatorio");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del mercado es obligatorio")
            .MaximumLength(120);

        RuleFor(x => x.LocationHint)
            .MaximumLength(180)
            .When(x => !string.IsNullOrWhiteSpace(x.LocationHint));
    }
}
