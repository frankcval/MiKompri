namespace MiKompri.Users.Domain.Users
{
    /// <summary>
    /// Rol de un usuario dentro de un grupo.
    /// EF lo persiste como string (<see cref="string"/>), por lo que cambiar
    /// los valores enteros no afecta a la base de datos.
    /// </summary>
    public enum GroupRole
    {
        Owner = 1,
        Admin = 2,
        Member = 3
    }
}
