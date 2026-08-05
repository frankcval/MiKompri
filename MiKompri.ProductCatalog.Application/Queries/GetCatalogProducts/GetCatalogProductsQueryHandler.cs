using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Queries.GetCatalogProducts;

public class GetCatalogProductsQueryHandler : IRequestHandler<GetCatalogProductsQuery, IReadOnlyCollection<CatalogProductDto>>
{
    private readonly ICatalogProductRepository _repository;

    public GetCatalogProductsQueryHandler(ICatalogProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<CatalogProductDto>> Handle(GetCatalogProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAsync(request.IncludeInactive, request.Search, cancellationToken);

        return products.Select(x => x.ToDto()).ToList();
    }
}
