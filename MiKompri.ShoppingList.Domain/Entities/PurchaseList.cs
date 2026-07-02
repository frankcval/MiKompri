
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
            Name = NormalizeName(name);
            OwnerId = ownerId != Guid.Empty
                ? ownerId
                : throw new InvalidOperationException("El propietario de la lista es obligatorio.");
            GroupId = groupId;
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("El nombre de la lista es obligatorio.");

            return name.Trim();
        }


        public void Rename(string newName)
        {
            var normalizedName = NormalizeName(newName);
            if (Name == normalizedName)
            {
                return;
            }

            Name = normalizedName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeGroup(Guid? newGroupId)
        {
            if (GroupId == newGroupId)
            {
                return;
            }

            GroupId = newGroupId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDescription(string? newDescription)
        {
            if (Description == newDescription)
            {
                return;
            }

            Description = newDescription;
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
            //  _items.Add(item);
            // Regla de negocio: no repetir producto en la misma lista
            if (_items.Any(i => i.ProductId == item.ProductId))
            {
                throw new InvalidOperationException(
                    $"El producto {item.ProductId} ya existe en esta lista de compra."
                );
            }

            // Aquí “adoptas” al hijo
            item.SetPurchaseList(this);

            _items.Add(item);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItem(Guid productId, string? newProductName, decimal? newPrice, int? newQuantity)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId)
                 ?? throw new InvalidOperationException("El item no existe en la lista");

            if (!string.IsNullOrWhiteSpace(newProductName))
                item.updateName(newProductName);

            if (newPrice.HasValue)
                item.updatePrice(newPrice.Value);

            if (newQuantity.HasValue && newQuantity.Value > 0)
            {
                item.updateQuantity(newQuantity.Value);
            }

            if (item.UpdatedAt.HasValue)
            {
                UpdatedAt = item.UpdatedAt.Value;
            }
        }

        public void DeleteItem(Guid productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId)
                 ?? throw new InvalidOperationException("El item no existe en la lista");

            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;

        }


        public void MarkItemAsPurchased(Guid productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId)
                        ?? throw new InvalidOperationException("El item no existe en la lista");
            if (item.MarkAsPurchased())
            {
                UpdatedAt = item.UpdatedAt;
            }
        }


    }
}
