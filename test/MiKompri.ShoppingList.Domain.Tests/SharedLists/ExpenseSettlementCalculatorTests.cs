using MiKompri.ShoppingList.Domain.Services;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class ExpenseSettlementCalculatorTests
    {
        [Fact]
        public void ComputeNetBalances_ShouldReturnExpectedBalances()
        {
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            var payments = new[]
            {
                (PaidBy: userA, Amount: 10m),
                (PaidBy: userB, Amount: 2m)
            };

            var shares = new[]
            {
                (ParticipantUserId: userA, ShareAmount: 6m),
                (ParticipantUserId: userB, ShareAmount: 6m)
            };

            var balances = ExpenseSettlementCalculator.ComputeNetBalances(payments, shares);

            Assert.Equal(4m, balances[userA]);
            Assert.Equal(-4m, balances[userB]);
        }
    }
}
