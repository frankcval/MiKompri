namespace MiKompri.ShoppingList.Application.DTOs
{
    public class SettlementSummaryDto
    {
        public Guid SharedListId { get; set; }
        public List<ParticipantBalanceDto> Balances { get; set; } = new();
    }

    public class ParticipantBalanceDto
    {
        public Guid UserId { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOwed { get; set; }
        public decimal NetBalance { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
