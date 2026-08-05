using FluentValidation;

namespace MiKompri.ProductCatalog.Application.Commands.RegisterProductPrice;

public class RegisterProductPriceCommandValidator : AbstractValidator<RegisterProductPriceCommand>
{
    public RegisterProductPriceCommandValidator()
    {
        RuleFor(x => x.CatalogProductId)
            .NotEmpty().WithMessage("catalogProductId es obligatorio");

        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("marketId es obligatorio");

        RuleFor(x => x.EffectiveDate)
            .Must(x => x != default)
            .WithMessage("effectiveDate es obligatorio");

        RuleFor(x => x.PriceAmount)
            .GreaterThan(0)
            .WithMessage("priceAmount debe ser mayor que cero");
    }
}
