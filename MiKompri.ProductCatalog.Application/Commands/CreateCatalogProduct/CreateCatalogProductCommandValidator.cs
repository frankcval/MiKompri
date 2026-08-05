using FluentValidation;

namespace MiKompri.ProductCatalog.Application.Commands.CreateCatalogProduct;

public class CreateCatalogProductCommandValidator : AbstractValidator<CreateCatalogProductCommand>
{
    public CreateCatalogProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(120);

        RuleFor(x => x.PurchaseUnit)
            .NotEmpty().WithMessage("La unidad de compra es obligatoria")
            .MaximumLength(30);
    }
}
