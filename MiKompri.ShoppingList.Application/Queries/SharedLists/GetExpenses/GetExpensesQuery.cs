using MediatR;
using MiKompri.ShoppingList.Application.Policies;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Queries.SharedLists.GetExpenses;

public sealed record GetExpensesQuery(Guid ListId) : IRequest<IReadOnlyList<ExpenseView>>;
public sealed record ExpenseView(Guid ExpenseId, Guid ItemId, string ProductName, decimal RealPaidPrice, string Currency, Guid PaidBy, Guid? PurchasedBy, Guid[] Participants, DateTime CreatedAt, DateTime? UpdatedAt, Guid? CreatedBy);

public sealed class GetExpensesHandler(PurchaseAccessPolicy access, IPurchaseListRepository repository) : IRequestHandler<GetExpensesQuery, IReadOnlyList<ExpenseView>>
{
    public async Task<IReadOnlyList<ExpenseView>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        var list = await access.RequireList(request.ListId, false, false, null, ct);
        if (!list.IsShared) throw new ForbiddenOperationException("La operación requiere una lista compartida.");
        var events = await repository.GetSharedListAuditEventsAsync(list.Id, ct);
        return list.Items.SelectMany(i => i.Expenses.Select(e => new ExpenseView(e.Id, i.Id, i.Name, e.RealPaidPrice, e.Currency, e.PaidBy, e.PurchasedBy, e.Participants.Select(p => p.ParticipantUserId).ToArray(), e.CreatedAt, e.UpdatedAt, events.FirstOrDefault(a => a.TargetEntityId == e.Id && a.ActionType == "ExpenseRecorded")?.ActorUserId))).OrderByDescending(e => e.CreatedAt).ToArray();
    }
}
