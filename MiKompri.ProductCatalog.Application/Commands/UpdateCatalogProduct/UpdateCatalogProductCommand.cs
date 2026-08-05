using MediatR;

namespace MiKompri.ProductCatalog.Application.Commands.UpdateCatalogProduct;

public record UpdateCatalogProductCommand(Guid CatalogProductId, string Name, string PurchaseUnit) : IRequest;
