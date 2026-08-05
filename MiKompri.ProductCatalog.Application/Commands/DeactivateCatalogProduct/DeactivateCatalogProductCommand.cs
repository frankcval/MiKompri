using MediatR;

namespace MiKompri.ProductCatalog.Application.Commands.DeactivateCatalogProduct;

public record DeactivateCatalogProductCommand(Guid CatalogProductId) : IRequest;
