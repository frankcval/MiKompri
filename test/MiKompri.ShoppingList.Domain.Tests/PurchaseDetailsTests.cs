using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests;

public class PurchaseDetailsTests
{
    [Fact]
    public void LocalTodayIsAcceptedEvenWhenItIsTheNextUtcCalendarDay()
    {
        var actor = Guid.NewGuid();
        var expense = new ItemExpenseRecord(Guid.NewGuid(), actor, null, 1m, new[] { actor });
        var today = new DateOnly(2030, 10, 30);
        expense.SetPurchaseDetails(null, today, today);
        Assert.Equal(today, expense.PurchasedOn);
        expense.SetPurchaseDetails(null, null, today);
        Assert.Null(expense.PurchasedOn);
    }

    [Fact]
    public void ActualDateAndMarketAreOptionalAndSeparateFromAudit_AndVersionChanges()
    {
        var actor = Guid.NewGuid();
        var expense = new ItemExpenseRecord(Guid.NewGuid(), actor, actor, 6m, new[] { actor });
        Assert.Null(expense.PurchasedOn);
        var version = expense.Version;
        var market = Guid.NewGuid();
        expense.SetPurchaseDetails(market, new DateOnly(2030, 10, 29), new DateOnly(2030, 10, 30));
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
        Assert.Throws<InvalidOperationException>(() => expense.SetPurchaseDetails(null, new DateOnly(2030, 10, 31), new DateOnly(2030, 10, 30)));
    }
}
