namespace MiKompri.ShoppingList.Domain.Entities
{
    public class SharedListAuditEvent
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid SharedPurchaseListId { get; private set; }
        public Guid ActorUserId { get; private set; }
        public string ActionType { get; private set; } = string.Empty;
        public string TargetEntityType { get; private set; } = string.Empty;
        public Guid TargetEntityId { get; private set; }
        public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;
        public string? Metadata { get; private set; }

        protected SharedListAuditEvent() { }

        public SharedListAuditEvent(Guid sharedPurchaseListId, Guid actorUserId, string actionType, string targetEntityType, Guid targetEntityId, string? metadata = null)
        {
            SharedPurchaseListId = sharedPurchaseListId;
            ActorUserId = actorUserId;
            ActionType = actionType;
            TargetEntityType = targetEntityType;
            TargetEntityId = targetEntityId;
            Metadata = metadata;
        }
    }
}
