using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests;

public class PurchaseDetailsTests
{
    [Fact]
    public void ActualDateAndMarketAreOptionalAndSeparateFromAudit_AndVersionChanges()
    {
        var actor = Guid.NewGuid();
        var expense = new ItemExpenseRecord(Guid.NewGuid(), actor, actor, 6m, new[] { actor });
        Assert.Null(expense.PurchasedOn);
        var version = expense.Version;
        var market = Guid.NewGuid();
        expense.SetPurchaseDetails(market, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        Assert.Equal(market, expense.MarketId);
        Assert.NotEqual(version, expense.Version);
        Assert.Equal(6m, expense.RealPaidPrice);
        Assert.Equal(6m, Assert.Single(expense.Participants).ShareAmount);
    }

    [Fact]
    public void FuturePurchaseDateIsRejected()
    {
        var actor = Guid.NewGuid();
        var expense = new ItemExpenseRecord(Guid.NewGuid(), actor, null, 1m, new[] { actor });
        Assert.Throws<InvalidOperationException>(() => expense.SetPurchaseDetails(null, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1)));
    }
}
