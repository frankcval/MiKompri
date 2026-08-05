using MediatR;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Application.Commands.UpdateCatalogProduct;

public class UpdateCatalogProductCommandHandler : IRequestHandler<UpdateCatalogProductCommand>
{
    private readonly ICatalogProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCatalogProductCommandHandler(ICatalogProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCatalogProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.CatalogProductId, cancellationToken)
                      ?? throw new KeyNotFoundException("Producto no encontrado.");

        var name = new ProductName(request.Name);
        var unit = request.PurchaseUnit.Trim();

        var exists = await _repository.ExistsActiveWithNameAndUnitAsync(name.NormalizedValue, unit, request.CatalogProductId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Ya existe un producto activo con el mismo nombre y unidad.");
        }

        product.UpdateDetails(name, unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
