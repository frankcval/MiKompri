using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Services;

public sealed class SystemCurrentDateProvider(TimeProvider? clock = null) : ICurrentDateProvider
{
    public DateOnly Today => DateOnly.FromDateTime((clock ?? TimeProvider.System).GetUtcNow().UtcDateTime);
}
