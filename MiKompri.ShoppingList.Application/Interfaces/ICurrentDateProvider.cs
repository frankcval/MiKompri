namespace MiKompri.ShoppingList.Application.Interfaces;

/// <summary>The caller's current civil date; callers without date context use UTC.</summary>
public interface ICurrentDateProvider
{
    DateOnly Today { get; }
}
