using MediatR;

namespace MiKompri.ProductCatalog.Application.Commands.DeactivateMarket;

public record DeactivateMarketCommand(Guid MarketId) : IRequest;
