using MediatR;

namespace MiKompri.ProductCatalog.Application.Commands.RegisterProductPrice;

public record RegisterProductPriceCommand(
    Guid CatalogProductId,
    Guid MarketId,
    DateOnly EffectiveDate,
    decimal PriceAmount) : IRequest<Guid>;
