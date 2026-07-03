namespace MiKompri.Users.Api.Models
{
    /// <summary>
    /// Body para POST /api/v1/groups/{groupId}/members.
    /// <c>Role</c> se acepta como string ("Member" | "Admin") y se parsea en el controller.
    /// </summary>
    public sealed class AddGroupMemberRequest
    {
        public Guid UserId { get; init; }
        public string Role { get; init; } = string.Empty;
    }
}
