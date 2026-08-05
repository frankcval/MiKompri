using MiKompri.ProductCatalog.Application.Interfaces;

namespace MiKompri.ProductCatalog.Infrastructure.Persistence;

public sealed class UnitOfWork(ProductCatalogDbContext context) : IUnitOfWork
{
    private readonly ProductCatalogDbContext _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
