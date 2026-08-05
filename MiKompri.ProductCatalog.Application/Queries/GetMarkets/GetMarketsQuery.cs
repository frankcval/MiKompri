using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;

namespace MiKompri.ProductCatalog.Application.Queries.GetMarkets;

public record GetMarketsQuery(bool IncludeInactive = false, string? Search = null) : IRequest<IReadOnlyCollection<MarketDto>>;
