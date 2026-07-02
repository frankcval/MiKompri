namespace MiKompri.Users.Application.Dtos
{
    public sealed class GroupDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public Guid OwnerId { get; init; }
        public string MyRole { get; init; } = string.Empty;
        public List<GroupMemberDto> Members { get; init; } = new();
    }
}
