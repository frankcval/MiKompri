using MediatR;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Policies;
using MiKompri.ShoppingList.Application.Services;

namespace MiKompri.ShoppingList.Application.Commands.PersonalPurchases;

public sealed record RegisterPersonalPurchase(Guid ListId, Guid ItemId, Guid OperationId, decimal RealPaidPrice, string Currency, Guid? MarketId, DateOnly? PurchasedOn) : IRequest<Guid>;
public sealed record UpdatePersonalPurchase(Guid ListId, Guid ItemId, Guid ExpenseId, Guid ExpectedVersion, decimal RealPaidPrice, string Currency, Guid? MarketId, DateOnly? PurchasedOn) : IRequest;
public sealed record DeletePersonalPurchase(Guid ListId, Guid ItemId, Guid ExpenseId) : IRequest;

public sealed class PersonalPurchaseHandler(PurchaseAccessPolicy access, IUnitOfWork unit, PurchaseOperationService operations, ICurrentDateProvider? dates = null)
    : IRequestHandler<RegisterPersonalPurchase, Guid>, IRequestHandler<UpdatePersonalPurchase>, IRequestHandler<DeletePersonalPurchase>
{
    private readonly ICurrentDateProvider _dates = dates ?? new SystemCurrentDateProvider();

    private async Task<Domain.Entities.PurchaseList> List(Guid id, CancellationToken ct)
    {
        var list = await access.RequireList(id, true, false, null, ct);
        if (list.IsShared) throw new ForbiddenOperationException("Usa el registro de gastos de la lista compartida.");
        return list;
    }

    public async Task<Guid> Handle(RegisterPersonalPurchase request, CancellationToken ct)
    {
        var list = await List(request.ListId, ct);
        var item = list.Items.SingleOrDefault(x => x.Id == request.ItemId) ?? throw new KeyNotFoundException("Producto no encontrado.");
        if (request.OperationId == Guid.Empty) throw new InvalidOperationException("La operación de compra es obligatoria.");
        PurchaseOperationService.Validate(request.RealPaidPrice, request.Currency);
        var participants = new[] { access.ActorId };
        var hash = PurchaseOperationService.Fingerprint(access.ActorId, access.ActorId, request.RealPaidPrice, request.Currency, participants, request.MarketId, request.PurchasedOn);
        var replay = await operations.ReplayAsync(request.OperationId, access.ActorId, list.Id, item.Id, hash, ct);
        if (replay.HasValue) return replay.Value;
        var expense = item.RegisterExpense(access.ActorId, access.ActorId, request.RealPaidPrice, participants, request.Currency);
        expense.SetPurchaseDetails(request.MarketId, request.PurchasedOn, _dates.Today);
        list.Touch();
        await operations.RecordAsync(request.OperationId, access.ActorId, list.Id, item.Id, expense.Id, hash, ct);
        await unit.SaveChangesAsync(ct);
        return expense.Id;
    }

    public async Task Handle(UpdatePersonalPurchase request, CancellationToken ct)
    {
        var list = await List(request.ListId, ct);
        var item = list.Items.SingleOrDefault(x => x.Id == request.ItemId) ?? throw new KeyNotFoundException("Producto no encontrado.");
        var expense = item.Expenses.SingleOrDefault(x => x.Id == request.ExpenseId) ?? throw new KeyNotFoundException("Compra no encontrada.");
        if (expense.Version != request.ExpectedVersion) throw new ResourceConflictException("La compra cambió. Actualiza los datos antes de corregirla.");
        PurchaseOperationService.Validate(request.RealPaidPrice, request.Currency);
        item.UpdateExpense(expense.Id, access.ActorId, access.ActorId, request.RealPaidPrice, new[] { access.ActorId }, request.Currency);
        expense.SetPurchaseDetails(request.MarketId, request.PurchasedOn, _dates.Today);
        list.Touch();
        await unit.SaveChangesAsync(ct);
    }

    public async Task Handle(DeletePersonalPurchase request, CancellationToken ct)
    {
        var list = await List(request.ListId, ct);
        var item = list.Items.SingleOrDefault(x => x.Id == request.ItemId) ?? throw new KeyNotFoundException("Producto no encontrado.");
        item.DeleteExpense(request.ExpenseId); list.Touch();
        await unit.SaveChangesAsync(ct);
    }
}
