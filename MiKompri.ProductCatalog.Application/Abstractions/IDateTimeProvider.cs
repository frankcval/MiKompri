namespace MiKompri.ProductCatalog.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly UtcToday { get; }
}
