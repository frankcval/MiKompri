namespace MiKompri.ShoppingList.Api.Models.SharedLists
{
    public class UpdateExpenseRequest
    {
        public Guid PaidBy { get; set; }
        public Guid? PurchasedBy { get; set; }
        public decimal RealPaidPrice { get; set; }
        public string Currency { get; set; } = "EUR";
        public List<Guid> Participants { get; set; } = new();
        public Guid? ExpectedVersion { get; set; }
        private Guid? _marketId;
        private DateOnly? _purchasedOn;
        public Guid? MarketId { get => _marketId; set { _marketId = value; MarketSpecified = true; } }
        public DateOnly? PurchasedOn { get => _purchasedOn; set { _purchasedOn = value; PurchasedOnSpecified = true; } }
        [System.Text.Json.Serialization.JsonIgnore]
        public bool MarketSpecified { get; private set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public bool PurchasedOnSpecified { get; private set; }
    }
}
