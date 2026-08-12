namespace MiKompri.ShoppingList.Application.DTOs
{
    public class SharedListAuditEventDto
    {
        public Guid Id { get; set; }
        public Guid SharedPurchaseListId { get; set; }
        public Guid ActorUserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string TargetEntityType { get; set; } = string.Empty;
        public Guid TargetEntityId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Metadata { get; set; }
    }
}
