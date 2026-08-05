using MediatR;

namespace MiKompri.ProductCatalog.Application.Commands.UpdateMarket;

public record UpdateMarketCommand(Guid MarketId, string Name, string? LocationHint) : IRequest;
