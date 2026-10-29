using MiKompri.ProductCatalog.Application.Abstractions;

namespace MiKompri.ProductCatalog.Infrastructure.Services;

public sealed class SystemDateTimeProvider(TimeProvider? clock = null) : IDateTimeProvider
{
    public DateTime UtcNow => (clock ?? TimeProvider.System).GetUtcNow().UtcDateTime;

    public DateOnly Today => DateOnly.FromDateTime((clock ?? TimeProvider.System).GetUtcNow().UtcDateTime);
}
