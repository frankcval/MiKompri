using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests;

public class PurchaseStateTests
{
    [Fact]
    public void ReversingPurchaseIsIdempotentAndPreservesExpenses()
    {
        var user = Guid.NewGuid();
        var list = new PurchaseList("Casa", user, Guid.NewGuid());
        var item = new ListItem(Guid.NewGuid(), "Pan", 2, 1, user);
        list.AddItem(item);
        item.RegisterExpense(user, user, 2, new[] { user });
        Assert.True(item.SetPurchaseState(true, user));
        var timestamp = item.UpdatedAt;
        Assert.False(item.SetPurchaseState(true, user));
        Assert.Equal(timestamp, item.UpdatedAt);
        Assert.True(item.SetPurchaseState(false, user));
        Assert.Single(item.Expenses);
        Assert.Equal(0, list.Progress.PurchasedItems);
    }

    [Fact]
    public void UnknownEstimateDoesNotReinterpretExistingZero()
    {
        var item = new ListItem(Guid.NewGuid(), "Pan", 0, 1);
        Assert.True(item.IsPriceKnown);
        item.SetEstimatedPrice(null, Guid.NewGuid());
        Assert.False(item.IsPriceKnown);
        item.SetEstimatedPrice(0, Guid.NewGuid());
        Assert.True(item.IsPriceKnown);
        Assert.Throws<InvalidOperationException>(() => item.SetEstimatedPrice(-1, Guid.NewGuid()));
    }
}
