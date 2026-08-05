using MediatR;
using MiKompri.ProductCatalog.Application.Dtos;
using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Application.Queries.GetProductPriceHistory;

public class GetProductPriceHistoryQueryHandler : IRequestHandler<GetProductPriceHistoryQuery, ProductPriceHistoryDto>
{
    private readonly ICatalogProductRepository _catalogProductRepository;
    private readonly IMarketRepository _marketRepository;
    private readonly IProductPriceRecordRepository _productPriceRecordRepository;

    public GetProductPriceHistoryQueryHandler(
        ICatalogProductRepository catalogProductRepository,
        IMarketRepository marketRepository,
        IProductPriceRecordRepository productPriceRecordRepository)
    {
        _catalogProductRepository = catalogProductRepository;
        _marketRepository = marketRepository;
        _productPriceRecordRepository = productPriceRecordRepository;
    }

    public async Task<ProductPriceHistoryDto> Handle(GetProductPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var product = await _catalogProductRepository.GetByIdAsync(request.CatalogProductId, cancellationToken)
                      ?? throw new KeyNotFoundException("Producto no encontrado.");

        var records = await _productPriceRecordRepository.GetByProductAsync(
            request.CatalogProductId,
            request.MarketId,
            request.From,
            request.To,
            cancellationToken);

        var ordered = records.OrderBy(x => x.EffectiveDate).ToList();
        var marketCache = new Dictionary<Guid, string>();

        foreach (var record in ordered)
        {
            if (marketCache.ContainsKey(record.MarketId))
            {
                continue;
            }

            var market = await _marketRepository.GetByIdAsync(record.MarketId, cancellationToken);
            marketCache[record.MarketId] = market?.Name ?? string.Empty;
        }

        return new ProductPriceHistoryDto
        {
            CatalogProductId = product.Id,
            CatalogProductName = product.Name.Value,
            Records = ordered.Select(record => new ProductPriceHistoryRecordDto
            {
                MarketId = record.MarketId,
                MarketName = marketCache.GetValueOrDefault(record.MarketId, string.Empty),
                EffectiveDate = record.EffectiveDate,
                PriceAmount = record.Price.Amount,
                Currency = record.Price.Currency
            }).ToList()
        };
    }
}
