using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;

namespace MiKompri.ProductCatalog.Application.Queries.GetCatalogProducts;

public record GetCatalogProductsQuery(bool IncludeInactive = false, string? Search = null) : IRequest<IReadOnlyCollection<CatalogProductDto>>;
