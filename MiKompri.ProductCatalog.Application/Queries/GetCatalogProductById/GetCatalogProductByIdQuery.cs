using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;

namespace MiKompri.ProductCatalog.Application.Queries.GetCatalogProductById;

public record GetCatalogProductByIdQuery(Guid CatalogProductId) : IRequest<CatalogProductDto>;
