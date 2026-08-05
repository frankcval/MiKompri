using MiKompri.ProductCatalog.Domain.Markets;

namespace MiKompri.ProductCatalog.Application.Dtos;

internal static class MarketMappings
{
    public static MarketDto ToDto(this Market market)
    {
        return new MarketDto
        {
            Id = market.Id,
            Name = market.Name,
            LocationHint = market.LocationHint,
            IsActive = market.IsActive,
            CreatedAt = market.CreatedAt,
            UpdatedAt = market.UpdatedAt
        };
    }
}
