namespace MiKompri.Users.Application.Dtos
{
    public sealed class GroupMemberDto
    {
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string Role { get; init; } = string.Empty;
        public DateTime JoinedAt { get; init; }
    }
}
