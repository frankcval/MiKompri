namespace MiKompri.Users.Domain.Abstractions
{
    /// <summary>
    /// Excepción lanzada cuando una operación está prohibida por la matriz de roles.
    /// Se mapea a HTTP 403 por el middleware de excepciones.
    /// No usar para reglas de negocio ordinarias (usar <see cref="InvalidOperationException"/> → 400).
    /// </summary>
    public sealed class ForbiddenOperationException : Exception
    {
        public ForbiddenOperationException(string message) : base(message) { }
    }
}
