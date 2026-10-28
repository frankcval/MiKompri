using MediatR;
using MiKompri.ShoppingList.Application.Commands.AddItemToList;
using MiKompri.ShoppingList.Application.Commands.CreateShoppingList;
using MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList;
using MiKompri.ShoppingList.Application.Commands.DeleteShoppinList;
using MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased;
using MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList;
using MiKompri.ShoppingList.Application.Commands.UpdateShoppingList;
using MiKompri.ShoppingList.Application.Commands.SharedLists.RegisterItemExpense;
using MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateItemExpense;
using MiKompri.ShoppingList.Application.Commands.SharedLists.DeleteItemExpense;
using MiKompri.ShoppingList.Application.Commands.SharedLists.AddSharedItem;
using MiKompri.ShoppingList.Application.Commands.SharedLists.UpdateSharedList;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Policies;
using MiKompri.ShoppingList.Application.Queries.GetAllShoppingLists;
using MiKompri.ShoppingList.Application.Queries.GetItemListById;
using MiKompri.ShoppingList.Application.Queries.GetShoppingListByGroupId;
using MiKompri.ShoppingList.Application.Queries.GetShoppingListById;

namespace MiKompri.ShoppingList.Application.Behavior;

// Protect legacy entry points in the application pipeline, including non-HTTP callers.
public sealed class ResourceAuthorizationBehavior<TRequest, TResponse>(PurchaseAccessPolicy access)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        switch (request)
        {
            case CreateShoppingListCommand c:
                access.RequireIdentity();
                if (c.OwnerId != access.ActorId) throw new ForbiddenOperationException("No puedes crear una lista para otra persona.");
                if (c.GroupId.HasValue) await access.RequireGroup(c.GroupId.Value, ct);
                break;
            case UpdateSharedListCommand c:
                await access.RequireList(c.SharedListId, true, true, null, ct); break;
            case AddSharedItemCommand c:
                await access.RequireList(c.SharedListId, true, false, null, ct); break;
            case RegisterItemExpenseCommand c:
                await access.RequireList(c.SharedListId, true, false, null, ct); break;
            case UpdateItemExpenseCommand c:
                await access.RequireList(c.SharedListId, true, true, null, ct); break;
            case DeleteItemExpenseCommand c:
                await access.RequireList(c.SharedListId, true, true, null, ct); break;
            case GetAllShoppingListsQuery:
                access.RequireIdentity(); break;
            case GetShoppingListByGroupIdQuery q:
                await access.RequireGroup(q.GroupId, ct); break;
            case GetShoppingListByIdQuery q:
                await access.RequireList(q.Id, false, false, null, ct); break;
            case GetItemListByIdQuery q:
                await access.RequireList(q.IdList, false, false, null, ct); break;
            case AddItemCommand c:
                var personal = await access.RequireList(c.ListId, true, false, null, ct);
                if (personal.IsShared) throw new ForbiddenOperationException("Añade productos compartidos desde el contrato de la lista de grupo.");
                break;
            case UpdateShoppingListCommand c:
                var renamed = await access.RequireList(c.ListId, true, true, null, ct);
                if (renamed.IsShared) throw new ForbiddenOperationException("Usa la operación de gestión de la lista compartida.");
                if (c.GroupId.HasValue && c.GroupId != renamed.GroupId)
                    throw new ForbiddenOperationException("No se puede cambiar el grupo de una lista desde esta operación.");
                break;
            case DeletePurchaseListCommand c:
                var deleted = await access.RequireList(c.ListId, true, true, null, ct);
                if (deleted.IsShared) throw new ForbiddenOperationException("Usa la operación de gestión de la lista compartida.");
                if (deleted.Items.Any(i => i.Expenses.Count != 0)) throw new ResourceConflictException("La lista tiene gastos. Ciérrala para conservar las cuentas.");
                break;
            case UpdateItemShoopingListCommand c:
                var editedList = await access.RequireList(c.listId, true, false, c.ProdId, ct);
                if (editedList.IsShared) throw new ForbiddenOperationException("Usa la operación de producto compartido.");
                break;
            case DeleteItemShoppingListCommand c:
                var list = await access.RequireList(c.ListId, true, false, c.ProductId, ct);
                if (list.IsShared) throw new ForbiddenOperationException("Usa la operación de producto compartido.");
                if (list.Items.Any(i => i.Id == c.ProductId && i.Expenses.Count != 0))
                    throw new ResourceConflictException("Este producto tiene gastos asociados. Corrige los gastos antes de eliminarlo.");
                break;
            case MarkItemAsPurchasedCommand c:
                await access.RequireList(c.ListId, true, false, null, ct); break;
        }
        return await next();
    }
}
