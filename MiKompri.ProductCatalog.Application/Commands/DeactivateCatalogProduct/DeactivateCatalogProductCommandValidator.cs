using FluentValidation;

namespace MiKompri.ProductCatalog.Application.Commands.DeactivateCatalogProduct;

public class DeactivateCatalogProductCommandValidator : AbstractValidator<DeactivateCatalogProductCommand>
{
    public DeactivateCatalogProductCommandValidator()
    {
        RuleFor(x => x.CatalogProductId)
            .NotEmpty().WithMessage("catalogProductId es obligatorio");
    }
}
