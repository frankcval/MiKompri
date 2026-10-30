namespace MiKompri.Users.Domain.Users;

public sealed record ProfileDetails(string DisplayName, string? Username, string? GivenName,
    string? FamilyName, string? PreferredLanguage, string DefaultListSort);

public sealed class ProfileConflictException : Exception
{
    public ProfileConflictException() : base("El perfil ha cambiado. Actualiza antes de guardar.") { }
}
