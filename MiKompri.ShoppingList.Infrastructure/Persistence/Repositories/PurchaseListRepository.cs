using Microsoft.EntityFrameworkCore;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Repositories
{
    public class PurchaseListRepository : IPurchaseListRepository
    {
        private readonly ShoppingListDbContext _context;

        public PurchaseListRepository(ShoppingListDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<PurchaseList>> GetByGroupAsync(Guid groupId)
        {

            return await _context.PurchaseList
                .Include(x => x.Items)  //Importante, ya que si no se coloca las entidades relacionadas se devuelven en NUll
                .Where(x => x.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<PurchaseList?> GetByIdAsync(Guid id)
        {
            return await _context.PurchaseList
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PurchaseList>> GetByOwnerAsync(Guid ownerId)
        {
            return await _context.PurchaseList
                .Include(x => x.Items)
                .Where(x => x.OwnerId == ownerId)
                .ToListAsync();
        }

        //retornar todas las listas
        public async Task<IEnumerable<PurchaseList>> GetAllAsync()
        {
            return await _context.PurchaseList
                .Include(x => x.Items)
                .ToListAsync();
        }
        public async Task AddAsync(PurchaseList list)
        {
            await _context.PurchaseList.AddAsync(list);
        }
        public async Task UpdateAsync(PurchaseList list)
        {
            _context.PurchaseList.Update(list);
            await Task.CompletedTask;
        }

        public async Task UpdateItemAsync(Guid listId, Guid itemId, string? name, decimal? price, int? quantity)
        {

            var list = await _context.PurchaseList
                .Include(p => p.Items)
                .FirstOrDefaultAsync(x => x.Id == listId);

            if (list is null)
                throw new KeyNotFoundException("Lista no encontrada.");

            /*
                // 2. Usar la lógica de dominio
            list.UpdateItem(itemId, name, price, quantity);
             */
            list.UpdateItem(itemId, name, price, quantity);

            _context.PurchaseList.Update(list);

        }

        public async Task DeleteItemAsync(Guid listId, Guid itemId)
        {
            var list = await _context.PurchaseList
                 .Include(p => p.Items)
                 .FirstOrDefaultAsync(x => x.Id == listId);
            if (list is null)
                throw new KeyNotFoundException("Lista no encontrada.");

            list.DeleteItem(itemId);

            _context.PurchaseList.Update(list);
        }

        public async Task DeleteAsync(Guid id)
        {
            var list = await _context.PurchaseList
         .Include(x => x.Items)
         .FirstOrDefaultAsync(x => x.Id == id);

            if (list is null)
                throw new KeyNotFoundException("La lista no existe.");

            _context.PurchaseList.Remove(list);
            // Ojo: NO llamamos a SaveChanges aquí, eso es responsabilidad del UnitOfWork
        }
    }
}
