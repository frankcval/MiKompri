using MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;
using Moq;

namespace MiKompri.ShoppingList.Application.Tests.Commands.MarkItemAsPurchased;

public class MarkItemAsPurchasedCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_MarkItemAsPurchased_And_SaveChanges_When_ListFound()
    {
        var repo = new Mock<IPurchaseListRepository>();
        var uow = new Mock<IUnitOfWork>();
        var list = new PurchaseList("Mi Lista", Guid.NewGuid());
        var item = new ListItem(Guid.NewGuid(), "Producto", 9.99m, 1);
        list.AddItem(item);
        Assert.NotEqual(item.Id, item.ProductId);
        repo.Setup(r => r.GetByIdAsync(list.Id)).ReturnsAsync(list);
        repo.Setup(r => r.UpdateAsync(list)).Returns(Task.CompletedTask);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await new MarkItemAsPurchasedCommandHandler(repo.Object, uow.Object)
            .Handle(new MarkItemAsPurchasedCommand(list.Id, item.Id), CancellationToken.None);

        Assert.True(item.IsPurchased);
        repo.Verify(r => r.UpdateAsync(list), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RejectsCatalogId_WithoutSaving()
    {
        var repo = new Mock<IPurchaseListRepository>();
        var uow = new Mock<IUnitOfWork>();
        var list = new PurchaseList("Mi Lista", Guid.NewGuid());
        var item = new ListItem(Guid.NewGuid(), "Producto", 9.99m, 1);
        list.AddItem(item);
        repo.Setup(r => r.GetByIdAsync(list.Id)).ReturnsAsync(list);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MarkItemAsPurchasedCommandHandler(repo.Object, uow.Object)
                .Handle(new MarkItemAsPurchasedCommand(list.Id, item.ProductId), CancellationToken.None));

        Assert.False(item.IsPurchased);
        repo.Verify(r => r.UpdateAsync(It.IsAny<PurchaseList>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TryMarkItemAsPurchasedInNullList_ThrowKeyNotFoundException_When_ListNotFound()
    {
        var repo = new Mock<IPurchaseListRepository>();
        var uow = new Mock<IUnitOfWork>();
        var listId = Guid.NewGuid();
        repo.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync((PurchaseList?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new MarkItemAsPurchasedCommandHandler(repo.Object, uow.Object)
                .Handle(new MarkItemAsPurchasedCommand(listId, Guid.NewGuid()), CancellationToken.None));
    }
}
