using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class SharedListAuditEventTests
    {
        [Fact]
        public void Constructor_ShouldCreateAuditEvent()
        {
            var listId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var evt = new SharedListAuditEvent(listId, actorId, "ExpenseRecorded", "ItemExpenseRecord", targetId, "{}");

            Assert.Equal(listId, evt.SharedPurchaseListId);
            Assert.Equal(actorId, evt.ActorUserId);
            Assert.Equal("ExpenseRecorded", evt.ActionType);
            Assert.Equal(targetId, evt.TargetEntityId);
        }
    }
}
