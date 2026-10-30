namespace MiKompri.Users.Domain.Users;
public sealed class UserConflictException : Exception
{
    public UserConflictException() : base("Hay un conflicto de cuenta o username.") { }
}
