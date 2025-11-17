
using MiKompri.ShoppingList.Domain.Abtractions;
using MiKompri.ShoppingList.Domain.ValueObjects;

namespace MiKompri.ShoppingList.Domain.Entities
{
    public class PurchaseList : Entity, IAggregateRoot
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public Guid OwnerId { get; private set; }
        public Guid? GroupId { get; private set; }

        private readonly List<ListItem> _items = new();
        public IReadOnlyCollection<ListItem> Items => _items.AsReadOnly();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        //  public ListProgress Progress => ListProgress.Create(_items.Count, _items.Count(i => i.IsPurchased));

        public ListProgress Progress => ListProgress.Create(_items.Count, _items.Count(i => i.IsPurchased));

        protected PurchaseList() { } // EF Core

        public PurchaseList(string name, Guid ownerId, Guid? groupId = null)
        {
            Name = name;
            OwnerId = ownerId;
            GroupId = groupId;
        }


        public void Rename(string newName)
        {
            Name = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        //calcular elementos comprados
        private double CompletionPercentage
        {
            get
            {
                if (_items.Count == 0)
                    return 0;

                var purchased = _items.Count(i => i.IsPurchased);
                return Math.Round((double)purchased / _items.Count * 100, 2);
            }
        }

        public void AddItem(ListItem item)
        {
            _items.Add(item);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItem(Guid itemId, string? newProductName, decimal? newPrice, int? newQuantity)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == itemId)
                 ?? throw new InvalidOperationException("El item no existe en la lista");

            if (!string.IsNullOrWhiteSpace(newProductName))
                item.updateName(newProductName);

            if (newPrice.HasValue)
                item.updatePrice(newPrice.Value);

            if (newQuantity.HasValue && newQuantity.Value > 0)
            {
                item.updateQuantity(newQuantity.Value);
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void DeleteItem(Guid itemId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == itemId)
                 ?? throw new InvalidOperationException("El item no existe en la lista");

            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;

        }


        public void MarkItemAsPurchased(Guid productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId)
                        ?? throw new ArgumentNullException("Producto no encontrado");
            item.MarkAsPurchased();
        }


    }
}
