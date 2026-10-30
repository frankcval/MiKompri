using MiKompri.ShoppingList.Application.Services;

namespace MiKompri.ShoppingList.Application.Tests;

public class CurrentDateProviderTests
{
    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public void NonHttpConsumersUseAnExplicitUtcFallbackWithAnInjectableClock()
    {
        var dates = new SystemCurrentDateProvider(new Clock(new DateTimeOffset(2030, 10, 29, 23, 30, 0, TimeSpan.Zero)));
        Assert.Equal(new DateOnly(2030, 10, 29), dates.Today);
    }
}
