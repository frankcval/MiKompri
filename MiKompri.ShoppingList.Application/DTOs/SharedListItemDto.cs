namespace MiKompri.ShoppingList.Application.DTOs
{
    public class SharedListItemDto : ListItemDto
    {
        public List<ItemExpenseRecordDto> Expenses { get; set; } = new();
    }

    public class ItemExpenseRecordDto
    {
        public Guid Id { get; set; }
        public Guid PaidBy { get; set; }
        public Guid? PurchasedBy { get; set; }
        public decimal RealPaidPrice { get; set; }
        public List<ExpenseParticipantDto> Participants { get; set; } = new();
    }

    public class ExpenseParticipantDto
    {
        public Guid ParticipantUserId { get; set; }
        public decimal ShareAmount { get; set; }
    }
}
