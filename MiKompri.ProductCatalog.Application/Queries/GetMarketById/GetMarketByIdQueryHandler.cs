using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Queries.GetMarketById;

public class GetMarketByIdQueryHandler : IRequestHandler<GetMarketByIdQuery, MarketDto>
{
    private readonly IMarketRepository _repository;

    public GetMarketByIdQueryHandler(IMarketRepository repository)
    {
        _repository = repository;
    }

    public async Task<MarketDto> Handle(GetMarketByIdQuery request, CancellationToken cancellationToken)
    {
        var market = await _repository.GetByIdAsync(request.MarketId, cancellationToken)
                     ?? throw new KeyNotFoundException("Mercado no encontrado.");

        return market.ToDto();
    }
}
