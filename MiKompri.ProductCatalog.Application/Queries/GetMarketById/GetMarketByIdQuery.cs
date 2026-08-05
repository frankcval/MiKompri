using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;

namespace MiKompri.ProductCatalog.Application.Queries.GetMarketById;

public record GetMarketByIdQuery(Guid MarketId) : IRequest<MarketDto>;
