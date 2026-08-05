using MediatR;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Commands.DeactivateCatalogProduct;

public class DeactivateCatalogProductCommandHandler : IRequestHandler<DeactivateCatalogProductCommand>
{
    private readonly ICatalogProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCatalogProductCommandHandler(ICatalogProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateCatalogProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.CatalogProductId, cancellationToken)
                      ?? throw new KeyNotFoundException("Producto no encontrado.");

        product.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
