using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Services;
using MiKompri.ShoppingList.Domain.Entities;
using Moq;

namespace MiKompri.ShoppingList.Application.Tests;

public class PurchaseOperationServiceTests
{
    [Fact]
    public async Task ReceiptCannotBeReplayedByAnotherActorOrAfterExpenseDeletion()
    {
        var store = new Mock<IPurchaseOperationStore>(); var operation = new PurchaseOperation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "hash");
        store.Setup(x => x.FindAsync(operation.Id, default)).ReturnsAsync(operation);
        store.Setup(x => x.ExpenseExistsAsync(operation.ExpenseId, default)).ReturnsAsync(true);
        var service = new PurchaseOperationService(store.Object);
        Assert.Equal(operation.ExpenseId, await service.ReplayAsync(operation.Id, operation.ActorId, operation.ListId, operation.ItemId, "hash", default));
        await Assert.ThrowsAsync<ResourceConflictException>(() => service.ReplayAsync(operation.Id, Guid.NewGuid(), operation.ListId, operation.ItemId, "hash", default));
        await Assert.ThrowsAsync<ResourceConflictException>(() => service.ReplayAsync(operation.Id, operation.ActorId, operation.ListId, operation.ItemId, "changed", default));
        store.Setup(x => x.ExpenseExistsAsync(operation.ExpenseId, default)).ReturnsAsync(false);
        await Assert.ThrowsAsync<ResourceConflictException>(() => service.ReplayAsync(operation.Id, operation.ActorId, operation.ListId, operation.ItemId, "hash", default));
    }

    [Fact]
    public void EquivalentParticipantOrderProducesSameFingerprint()
    {
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        Assert.Equal(PurchaseOperationService.Fingerprint(a, null, 2m, "eur", new[] { a, b }, null, null), PurchaseOperationService.Fingerprint(a, null, 2.00m, "EUR", new[] { b, a }, null, null));
    }
}
