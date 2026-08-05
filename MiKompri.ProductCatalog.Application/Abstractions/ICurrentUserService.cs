namespace MiKompri.ProductCatalog.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
