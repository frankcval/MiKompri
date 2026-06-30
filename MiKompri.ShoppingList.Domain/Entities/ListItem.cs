
using MiKompri.ShoppingList.Domain.Abtractions;

namespace MiKompri.ShoppingList.Domain.Entities
{
    public class ListItem 
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; } // Referencia al ProductService
        public string Name { get; private set; } // Copia para visualización rápida
        public decimal Price { get; private set; } // Precio del momento
        public int Quantity { get; private set; }
        public bool IsPurchased { get; private set; }

        public Guid PurchaseListId { get; private set; }         // FK a PurchaseList
        public PurchaseList PurchaseList { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        public ListItem()
        {

        }


        public ListItem(Guid productId, string name, decimal price, int quantity)
        {
            ProductId = productId != Guid.Empty
                ? productId
                : throw new InvalidOperationException("El producto del item es obligatorio.");
            Name = NormalizeName(name);
            Price = NormalizePrice(price);
            Quantity = NormalizeQuantity(quantity);
            IsPurchased = false;
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("El nombre del item es obligatorio.");

            return name.Trim();
        }

        private static decimal NormalizePrice(decimal price)
        {
            if (price < 0)
                throw new InvalidOperationException("El precio del item no puede ser negativo.");

            return price;
        }

        private static int NormalizeQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("La cantidad del item debe ser mayor que cero.");

            return quantity;
        }

        internal void SetPurchaseList(PurchaseList list)
        {
            PurchaseList = list ?? throw new ArgumentNullException(nameof(list));
            PurchaseListId = list.Id;
        }

        public void updateName(string name)
        {
            var normalizedName = NormalizeName(name);
            if (Name == normalizedName)
            {
                return;
            }

            Name = normalizedName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void updatePrice(decimal price)
        {
            var normalizedPrice = NormalizePrice(price);
            if (Price == normalizedPrice)
            {
                return;
            }

            Price = normalizedPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void updateQuantity(int quantity)
        {
            var normalizedQuantity = NormalizeQuantity(quantity);
            if (Quantity == normalizedQuantity)
            {
                return;
            }

            Quantity = normalizedQuantity;
            UpdatedAt = DateTime.UtcNow;
        }


        public bool MarkAsPurchased()
        {
            if (IsPurchased)
            {
                return false;
            }

            IsPurchased = true;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
    }
}
