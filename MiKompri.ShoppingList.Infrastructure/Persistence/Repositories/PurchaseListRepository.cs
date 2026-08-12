using Microsoft.EntityFrameworkCore;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;
using System.Threading;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Repositories
{
    public class PurchaseListRepository : IPurchaseListRepository
    {
        private readonly ShoppingListDbContext _context;
        private IQueryable<PurchaseList> PurchaseListsWithItems => _context.PurchaseList
            .Include(p => p.Items)
                .ThenInclude(i => i.Expenses)
                    .ThenInclude(e => e.Participants);

        public PurchaseListRepository(ShoppingListDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<PurchaseList>> GetByGroupAsync(Guid groupId)
        {
            return await PurchaseListsWithItems
                .Where(x => x.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<PurchaseList?> GetByIdAsync(Guid id)
        {
            return await PurchaseListsWithItems
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PurchaseList>> GetByOwnerAsync(Guid ownerId)
        {
            return await PurchaseListsWithItems
                .Where(x => x.OwnerId == ownerId)
                .ToListAsync();
        }

        //retornar todas las listas
        public async Task<IEnumerable<PurchaseList>> GetAllAsync()
        {
            return await PurchaseListsWithItems
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
            var list = await PurchaseListsWithItems
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
            var list = await PurchaseListsWithItems
                 .FirstOrDefaultAsync(x => x.Id == listId);
            if (list is null)
                throw new KeyNotFoundException("Lista no encontrada.");

            list.DeleteItem(itemId);

            _context.PurchaseList.Update(list);
        }

        public async Task DeleteAsync(Guid id)
        {
            var list = await PurchaseListsWithItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (list is null)
                throw new KeyNotFoundException("La lista no existe.");

            _context.PurchaseList.Remove(list);
            // Ojo: NO llamamos a SaveChanges aquí, eso es responsabilidad del UnitOfWork
            // _context.PurchaseList.Update(list);


        }

        public async Task AddItemAsync(Guid listId, ListItem item, CancellationToken cancellationToken)
        {
            var list = await PurchaseListsWithItems
                .FirstOrDefaultAsync(x => x.Id == listId, cancellationToken);

            if (list is null)
                throw new KeyNotFoundException("Lista no encontrada.");

            // 2. Lógica de dominio (incluye SetPurchaseList + validaciones)
            list.AddItem(item);   // aquí dentro: item.SetPurchaseList(this);
            // El SaveChangesAsync lo hará el UnitOfWork / DbContext fuera de aquí.
        }

        public async Task<ListItem?> GetItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken)
        {
            return await _context.ListItems
                .Include(i => i.Expenses)
                    .ThenInclude(e => e.Participants)
                .Where(i => i.PurchaseListId == listId && i.Id == itemId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddSharedListAuditEventAsync(SharedListAuditEvent auditEvent)
        {
            await _context.SharedListAuditEvents.AddAsync(auditEvent);
        }

        public async Task<IReadOnlyCollection<SharedListAuditEvent>> GetSharedListAuditEventsAsync(Guid sharedListId, CancellationToken cancellationToken)
        {
            return await _context.SharedListAuditEvents
                .Where(x => x.SharedPurchaseListId == sharedListId)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }
    }
}
