using FluentValidation;

namespace MiKompri.ProductCatalog.Application.Commands.DeactivateMarket;

public class DeactivateMarketCommandValidator : AbstractValidator<DeactivateMarketCommand>
{
    public DeactivateMarketCommandValidator()
    {
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("marketId es obligatorio");
    }
}
