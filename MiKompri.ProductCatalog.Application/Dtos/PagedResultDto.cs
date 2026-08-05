namespace MiKompri.ProductCatalog.Application.Dtos;

public sealed record PagedResultDto<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
