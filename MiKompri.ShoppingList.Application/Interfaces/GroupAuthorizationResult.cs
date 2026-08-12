namespace MiKompri.ShoppingList.Application.Interfaces
{
    public sealed record GroupAuthorizationResult(bool IsMember, bool IsActive, GroupRole? Role)
    {
        public bool IsAuthorizedMember => IsMember && IsActive;
    }
}
