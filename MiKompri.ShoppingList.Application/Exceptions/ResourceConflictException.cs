namespace MiKompri.ShoppingList.Application.Exceptions;

public sealed class ResourceConflictException(string message) : Exception(message);
