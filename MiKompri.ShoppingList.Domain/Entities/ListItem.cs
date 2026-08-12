
using MiKompri.ShoppingList.Domain.Abtractions;

namespace MiKompri.ShoppingList.Domain.Entities
{
    public class ListItem 
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ProductId { get; private set; } // Referencia al ProductService
        public string Name { get; private set; } // Copia para visualización rápida
        public decimal Price { get; private set; } // Precio del momento
        public int Quantity { get; private set; }
        public bool IsPurchased { get; private set; }

        public Guid PurchaseListId { get; private set; }         // FK a PurchaseList
        public PurchaseList PurchaseList { get; private set; } = null!;
        public Guid AddedBy { get; private set; }
        public Guid UpdatedBy { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public List<ItemExpenseRecord> Expenses { get; private set; } = new();

        public ListItem()
        {

        }


        public ListItem(Guid productId, string name, decimal price, int quantity)
            : this(productId, name, price, quantity, Guid.Empty)
        {
        }

        public ListItem(Guid productId, string name, decimal price, int quantity, Guid addedBy)
        {
            ProductId = productId != Guid.Empty
                ? productId
                : throw new InvalidOperationException("El producto del item es obligatorio.");
            Name = NormalizeName(name);
            Price = NormalizePrice(price);
            Quantity = NormalizeQuantity(quantity);
            AddedBy = addedBy;
            UpdatedBy = addedBy;
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
            updateName(name, UpdatedBy);
        }

        public void updateName(string name, Guid updatedBy)
        {
            var normalizedName = NormalizeName(name);
            if (Name == normalizedName)
            {
                return;
            }

            Name = normalizedName;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void updatePrice(decimal price)
        {
            updatePrice(price, UpdatedBy);
        }

        public void updatePrice(decimal price, Guid updatedBy)
        {
            var normalizedPrice = NormalizePrice(price);
            if (Price == normalizedPrice)
            {
                return;
            }

            Price = normalizedPrice;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void updateQuantity(int quantity)
        {
            updateQuantity(quantity, UpdatedBy);
        }

        public void updateQuantity(int quantity, Guid updatedBy)
        {
            var normalizedQuantity = NormalizeQuantity(quantity);
            if (Quantity == normalizedQuantity)
            {
                return;
            }

            Quantity = normalizedQuantity;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }


        public bool MarkAsPurchased()
        {
            return MarkAsPurchased(UpdatedBy);
        }

        public bool MarkAsPurchased(Guid updatedBy)
        {
            if (IsPurchased)
            {
                return false;
            }

            IsPurchased = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public ItemExpenseRecord RegisterExpense(Guid paidBy, Guid? purchasedBy, decimal realPaidPrice, IEnumerable<Guid> participants, string? currency = null)
        {
            var expense = new ItemExpenseRecord(Id, paidBy, purchasedBy, realPaidPrice, participants, currency);
            Expenses.Add(expense);
            UpdatedAt = DateTime.UtcNow;
            return expense;
        }

        public void UpdateExpense(Guid expenseId, Guid paidBy, Guid? purchasedBy, decimal realPaidPrice, IEnumerable<Guid> participants, string? currency = null)
        {
            var expense = Expenses.FirstOrDefault(x => x.Id == expenseId)
                ?? throw new InvalidOperationException("El gasto no existe en el ítem.");

            expense.Update(paidBy, purchasedBy, realPaidPrice, participants, currency);
            UpdatedAt = DateTime.UtcNow;
        }

        public void DeleteExpense(Guid expenseId)
        {
            var expense = Expenses.FirstOrDefault(x => x.Id == expenseId)
                ?? throw new InvalidOperationException("El gasto no existe en el ítem.");

            Expenses.Remove(expense);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
