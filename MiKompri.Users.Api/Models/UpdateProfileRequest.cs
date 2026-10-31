namespace MiKompri.Users.Api.Models
{
    public sealed class UpdateProfileRequest
    {
        public string DisplayName { get; init; } = string.Empty;
        [System.Text.Json.Serialization.JsonExtensionData]
        public Dictionary<string, System.Text.Json.JsonElement>? Fields { get; init; }
    }
}
