
using MiKompri.ShoppingList.Domain.Abtractions;

namespace MiKompri.ShoppingList.Domain.Entities
{
    public class ListItem : Entity
    {
        public Guid ProductId { get; private set; } // Referencia al ProductService
        public string Name { get; private set; } // Copia para visualización rápida
        public decimal Price { get; private set; } // Precio del momento
        public int Quantity { get; private set; }
        public bool IsPurchased { get; private set; }

        public ListItem()
        {

        }

        public ListItem(Guid productId, string name, decimal price, int quantity)
        {
            ProductId = productId;
            Name = name;
            Price = price;
            Quantity = quantity;
            IsPurchased = false;
        }

        public void updateName(string name)
        {
            Name = name;
        }

        public void updatePrice(decimal price)
        {
            Price = price;
        }

        public void updateQuantity(int quantity)
        {
            Quantity = quantity;
        }


        public void MarkAsPurchased() => IsPurchased = true;
    }
}
