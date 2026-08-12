namespace MiKompri.ShoppingList.Application.Exceptions
{
    /// <summary>
    /// Lanzada cuando un servicio dependiente (p. ej. Users API) no está disponible
    /// o responde con un error inesperado (timeout, 5xx).
    /// Se mapea a HTTP 503 Service Unavailable en ExceptionHandlingMiddleware.
    /// </summary>
    public sealed class DependencyUnavailableException : Exception
    {
        public DependencyUnavailableException(string message) : base(message) { }
        public DependencyUnavailableException(string message, Exception inner) : base(message, inner) { }
    }
}
