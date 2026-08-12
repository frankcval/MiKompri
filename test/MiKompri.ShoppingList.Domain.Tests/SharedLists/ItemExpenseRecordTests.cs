using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class ItemExpenseRecordTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenPaidByIsEmpty()
        {
            var participants = new[] { Guid.NewGuid() };

            Assert.Throws<InvalidOperationException>(() =>
                new ItemExpenseRecord(Guid.NewGuid(), Guid.Empty, null, 10m, participants));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenParticipantsAreEmpty()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new ItemExpenseRecord(Guid.NewGuid(), Guid.NewGuid(), null, 10m, Array.Empty<Guid>()));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenParticipantsContainDuplicates()
        {
            var userId = Guid.NewGuid();

            Assert.Throws<InvalidOperationException>(() =>
                new ItemExpenseRecord(Guid.NewGuid(), userId, null, 10m, new[] { userId, userId }));
        }

        [Fact]
        public void Constructor_ShouldPreserveAccounting_WhenPayerIsNotParticipant()
        {
            var paidBy = Guid.NewGuid();
            var participantA = Guid.NewGuid();
            var participantB = Guid.NewGuid();

            var expense = new ItemExpenseRecord(
                Guid.NewGuid(),
                paidBy,
                null,
                10m,
                new[] { participantA, participantB });

            Assert.Equal(2, expense.Participants.Count);
            Assert.Equal(10m, expense.Participants.Sum(x => x.ShareAmount));
        }
    }
}
