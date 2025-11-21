using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Interfaces
{
    public interface IPurchaseListRepository
    {
        Task AddAsync(PurchaseList list);
        Task<PurchaseList?> GetByIdAsync(Guid id);
        Task<IEnumerable<PurchaseList>> GetByOwnerAsync(Guid ownerId);
        Task<IEnumerable<PurchaseList>> GetByGroupAsync(Guid groupId);
        Task<IEnumerable<PurchaseList>> GetAllAsync();
        Task UpdateAsync(PurchaseList list);

        Task DeleteAsync(Guid id);

        // Conveniencia: operar ítems respetando el agregado internamente
        public Task AddItemAsync(Guid listId, ListItem item, CancellationToken cancellationToken);
        public Task UpdateItemAsync(Guid listId, Guid itemId, string? name, decimal? price, int? quantity);
        public Task DeleteItemAsync(Guid listId, Guid itemId);
        //Task UpdateItemAsync(Guid listId, Guid itemId, string name, decimal price);  //por principios SOLID es mejor hacer un repo especializado, un atajo
        public Task<ListItem?> GetItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken);
    }
}
