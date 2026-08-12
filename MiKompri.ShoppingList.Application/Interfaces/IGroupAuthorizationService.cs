namespace MiKompri.ShoppingList.Application.Interfaces
{
    public interface IGroupAuthorizationService
    {
        Task<GroupAuthorizationResult> GetMembershipAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);
    }
}
