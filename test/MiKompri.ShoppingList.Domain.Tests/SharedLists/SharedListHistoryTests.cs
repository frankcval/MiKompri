using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class SharedListHistoryTests
    {
        [Fact]
        public void AuditEvents_ShouldPreserveActorIdentity_WhenMembershipChangesOutsideContext()
        {
            var listId = Guid.NewGuid();
            var formerMemberId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var auditEvent = new SharedListAuditEvent(
                listId,
                formerMemberId,
                "ExpenseRecorded",
                nameof(ItemExpenseRecord),
                eventId,
                "{\"note\":\"historic\"}");

            Assert.Equal(formerMemberId, auditEvent.ActorUserId);
            Assert.Equal(listId, auditEvent.SharedPurchaseListId);
            Assert.Equal("ExpenseRecorded", auditEvent.ActionType);
        }
    }
}
