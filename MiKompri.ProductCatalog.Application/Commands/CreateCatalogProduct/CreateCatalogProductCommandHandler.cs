using MediatR;
using MiKompri.ProductCatalog.Application.Interfaces;
using MiKompri.ProductCatalog.Domain.Products;
using MiKompri.ProductCatalog.Domain.Products.ValueObjects;

namespace MiKompri.ProductCatalog.Application.Commands.CreateCatalogProduct;

public class CreateCatalogProductCommandHandler : IRequestHandler<CreateCatalogProductCommand, Guid>
{
    private readonly ICatalogProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCatalogProductCommandHandler(ICatalogProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCatalogProductCommand request, CancellationToken cancellationToken)
    {
        var name = new ProductName(request.Name);
        var unit = request.PurchaseUnit.Trim();

        var exists = await _repository.ExistsActiveWithNameAndUnitAsync(name.NormalizedValue, unit, null, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Ya existe un producto activo con el mismo nombre y unidad.");
        }

        var product = new CatalogProduct(name, unit);

        await _repository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
