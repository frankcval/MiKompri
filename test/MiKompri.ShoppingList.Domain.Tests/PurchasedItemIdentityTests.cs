using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests;

public class PurchasedItemIdentityTests
{
    [Fact]
    public void Mark_UsesListItemId_NotCatalogProductId()
    {
        var list = new PurchaseList("Compra", Guid.NewGuid());
        var item = new ListItem(Guid.NewGuid(), "Arroz", 2m, 1);
        list.AddItem(item);
        Assert.NotEqual(item.ProductId, item.Id);

        list.MarkItemAsPurchased(item.Id);

        Assert.True(item.IsPurchased);
        Assert.Equal(1, list.Progress.PurchasedItems);
        Assert.Equal(item.UpdatedAt, list.UpdatedAt);
    }

    [Fact]
    public void Mark_DoesNotInterpretAnotherItemsProductId_AsItemIdentity()
    {
        var list = new PurchaseList("Compra", Guid.NewGuid());
        var first = new ListItem(Guid.NewGuid(), "Arroz", 2m, 1);
        var second = new ListItem(first.Id, "Leche", 3m, 1);
        list.AddItem(first);
        list.AddItem(second);

        list.MarkItemAsPurchased(first.Id);

        Assert.True(first.IsPurchased);
        Assert.False(second.IsPurchased);
    }

    [Fact]
    public void Mark_RejectsCatalogIdentityWithoutChangingProgress()
    {
        var list = new PurchaseList("Compra", Guid.NewGuid());
        var item = new ListItem(Guid.NewGuid(), "Arroz", 2m, 1);
        list.AddItem(item);

        Assert.Throws<InvalidOperationException>(() => list.MarkItemAsPurchased(item.ProductId));
        Assert.False(item.IsPurchased);
        Assert.Equal(0, list.Progress.PurchasedItems);
    }
}
