using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Queries.GetMarkets;

public class GetMarketsQueryHandler : IRequestHandler<GetMarketsQuery, IReadOnlyCollection<MarketDto>>
{
    private readonly IMarketRepository _repository;

    public GetMarketsQueryHandler(IMarketRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<MarketDto>> Handle(GetMarketsQuery request, CancellationToken cancellationToken)
    {
        var markets = await _repository.GetAsync(request.IncludeInactive, request.Search, cancellationToken);
        return markets.Select(x => x.ToDto()).ToList();
    }
}
