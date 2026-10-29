namespace MiKompri.ShoppingList.Api.Models.SharedLists
{
    public class RegisterExpenseRequest
    {
        public Guid PaidBy { get; set; }
        public Guid? PurchasedBy { get; set; }
        public decimal RealPaidPrice { get; set; }
        public string Currency { get; set; } = "EUR";
        public List<Guid> Participants { get; set; } = new();
        public Guid? OperationId { get; set; }
        public Guid? MarketId { get; set; }
        public DateOnly? PurchasedOn { get; set; }
    }
}
