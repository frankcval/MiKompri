using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class ExpenseParticipantTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenExpenseIdIsEmpty()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new ExpenseParticipant(Guid.Empty, Guid.NewGuid(), 10m));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenParticipantIdIsEmpty()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new ExpenseParticipant(Guid.NewGuid(), Guid.Empty, 10m));
        }
    }
}
