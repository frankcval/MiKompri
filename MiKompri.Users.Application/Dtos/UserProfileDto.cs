namespace MiKompri.Users.Application.Dtos
{
    public sealed class UserProfileDto
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string IdentityProvider { get; init; } = string.Empty;
        public string ExternalUserId { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
