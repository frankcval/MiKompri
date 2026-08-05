using FluentValidation;

namespace MiKompri.ProductCatalog.Application.Commands.UpdateCatalogProduct;

public class UpdateCatalogProductCommandValidator : AbstractValidator<UpdateCatalogProductCommand>
{
    public UpdateCatalogProductCommandValidator()
    {
        RuleFor(x => x.CatalogProductId)
            .NotEmpty().WithMessage("catalogProductId es obligatorio");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(120);

        RuleFor(x => x.PurchaseUnit)
            .NotEmpty().WithMessage("La unidad de compra es obligatoria")
            .MaximumLength(30);
    }
}
