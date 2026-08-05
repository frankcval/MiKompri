using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;

namespace MiKompri.ProductCatalog.Application.Queries.GetProductPriceHistory;

public record GetProductPriceHistoryQuery(
    Guid CatalogProductId,
    Guid? MarketId,
    DateOnly? From,
    DateOnly? To) : IRequest<ProductPriceHistoryDto>;
