using MediatR;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Policies;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Commands.ManagePurchase;

public sealed record SetPurchaseStateCommand(Guid ListId, Guid ItemId, bool IsPurchased) : IRequest;
public sealed record EditSharedItemCommand(Guid ListId, Guid ItemId, int Quantity, decimal? EstimatedPrice) : IRequest;
public sealed record RemoveSharedItemCommand(Guid ListId, Guid ItemId) : IRequest;
public sealed record RemoveSharedListCommand(Guid ListId) : IRequest;

public sealed class ManagePurchaseHandler(IPurchaseListRepository repository, IUnitOfWork unit, PurchaseAccessPolicy access)
    : IRequestHandler<SetPurchaseStateCommand>, IRequestHandler<EditSharedItemCommand>, IRequestHandler<RemoveSharedItemCommand>, IRequestHandler<RemoveSharedListCommand>
{
    public async Task Handle(SetPurchaseStateCommand request, CancellationToken ct)
    {
        var list = await access.RequireList(request.ListId, true, false, null, ct);
        var item = list.Items.SingleOrDefault(i => i.Id == request.ItemId) ?? throw new KeyNotFoundException("El producto no está disponible.");
        if (!item.SetPurchaseState(request.IsPurchased, access.ActorId)) return;
        list.Touch();
        if (list.IsShared) await Audit(list.Id, item.Id, "PurchaseStateChanged");
        await repository.UpdateAsync(list);
        await unit.SaveChangesAsync(ct);
    }

    public async Task Handle(EditSharedItemCommand request, CancellationToken ct)
    {
        var list = await access.RequireList(request.ListId, true, false, request.ItemId, ct);
        if (!list.IsShared) throw new ForbiddenOperationException("La operación requiere una lista compartida.");
        var item = list.Items.SingleOrDefault(i => i.Id == request.ItemId) ?? throw new KeyNotFoundException("El producto no está disponible.");
        item.updateQuantity(request.Quantity, access.ActorId);
        item.SetEstimatedPrice(request.EstimatedPrice, access.ActorId);
        list.Touch();
        await Audit(list.Id, item.Id, "ItemUpdated");
        await repository.UpdateAsync(list);
        await unit.SaveChangesAsync(ct);
    }

    public async Task Handle(RemoveSharedItemCommand request, CancellationToken ct)
    {
        var list = await access.RequireList(request.ListId, true, false, request.ItemId, ct);
        if (!list.IsShared) throw new ForbiddenOperationException("La operación requiere una lista compartida.");
        var item = list.Items.SingleOrDefault(i => i.Id == request.ItemId) ?? throw new KeyNotFoundException("El producto no está disponible.");
        if (item.Expenses.Count != 0) throw new ResourceConflictException("El producto tiene gastos. Corrígelos antes de eliminarlo.");
        list.DeleteItem(item.ProductId);
        await Audit(list.Id, item.Id, "ItemDeleted");
        await repository.UpdateAsync(list);
        await unit.SaveChangesAsync(ct);
    }

    public async Task Handle(RemoveSharedListCommand request, CancellationToken ct)
    {
        var list = await access.RequireList(request.ListId, true, true, null, ct);
        if (!list.IsShared) throw new ForbiddenOperationException("La operación requiere una lista compartida.");
        if (list.Items.Any(i => i.Expenses.Count != 0)) throw new ResourceConflictException("La lista tiene gastos. Ciérrala para conservar las cuentas.");
        await Audit(list.Id, list.Id, "ListDeleted", nameof(PurchaseList));
        await repository.DeleteAsync(list.Id);
        await unit.SaveChangesAsync(ct);
    }

    private Task Audit(Guid listId, Guid resourceId, string action, string resourceType = nameof(ListItem)) => repository.AddSharedListAuditEventAsync(new SharedListAuditEvent(listId, access.ActorId, action, resourceType, resourceId));
}
