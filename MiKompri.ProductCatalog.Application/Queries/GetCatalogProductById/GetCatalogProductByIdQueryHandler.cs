using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Queries.GetCatalogProductById;

public class GetCatalogProductByIdQueryHandler : IRequestHandler<GetCatalogProductByIdQuery, CatalogProductDto>
{
    private readonly ICatalogProductRepository _repository;

    public GetCatalogProductByIdQueryHandler(ICatalogProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatalogProductDto> Handle(GetCatalogProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.CatalogProductId, cancellationToken)
                      ?? throw new KeyNotFoundException("Producto no encontrado.");

        return product.ToDto();
    }
}
