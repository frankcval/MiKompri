using FluentValidation;
namespace MiKompri.ProductCatalog.Application.Queries.GetProductPriceHistory;

public sealed class GetProductPriceHistoryQueryValidator : AbstractValidator<GetProductPriceHistoryQuery>
{
    public GetProductPriceHistoryQueryValidator()
    {
        RuleFor(x => x.CatalogProductId).NotEmpty();
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("La fecha inicial debe ser anterior o igual a la final.");
    }
}
