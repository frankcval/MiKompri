namespace MiKompri.ShoppingList.Application.Interfaces
{
    public interface IUserIdentityResolver
    {
        Task<Guid?> ResolveCurrentUserIdAsync(CancellationToken cancellationToken);
    }
}
