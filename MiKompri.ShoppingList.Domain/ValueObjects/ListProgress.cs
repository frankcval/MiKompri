
using MiKompri.ShoppingList.Domain.Abtractions;

namespace MiKompri.ShoppingList.Domain.ValueObjects
{
    /// <summary>
    /// Muestra progreso de compra de una lista 
    /// </summary>
    /// <param name="TotalItems">Cantidad de Items</param>
    /// <param name="PurchasedItems">Cantidad Items Comprados</param>
    public class ListProgress(int TotalItems, int PurchasedItems) : ValueObject
    {
        public int TotalItems { get; } = TotalItems >= 0
            ? TotalItems
            : throw new InvalidOperationException("El total de items no puede ser negativo.");

        public int PurchasedItems { get; } = PurchasedItems >= 0 && PurchasedItems <= TotalItems
            ? PurchasedItems
            : throw new InvalidOperationException("La cantidad de items comprados no es válida.");

        public int PendingItems => TotalItems - PurchasedItems;
        public double Percentage => TotalItems == 0 ? 0 : Math.Round((double)PurchasedItems / TotalItems * 100, 2);
        public static ListProgress Create(int total, int purchased)
                => new ListProgress(total, purchased);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Using a yield return statement to return each element one at a time
            yield return Percentage;

        }
    }

}
