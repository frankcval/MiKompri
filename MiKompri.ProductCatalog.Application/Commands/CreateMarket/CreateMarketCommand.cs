using MediatR;

namespace MiKompri.ProductCatalog.Application.Commands.CreateMarket;

public record CreateMarketCommand(string Name, string? LocationHint) : IRequest<Guid>;
