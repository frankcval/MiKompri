using MediatR;

namespace MiKompri.ProductCatalog.Application.Commands.CreateCatalogProduct;

public record CreateCatalogProductCommand(string Name, string PurchaseUnit) : IRequest<Guid>;
