namespace MiKompri.ShoppingList.Application.DTOs
{
    public class SettlementProposalDto
    {
        public Guid SharedListId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public List<SettlementTransferDto> Transfers { get; set; } = new();
    }

    public class SettlementTransferDto
    {
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public decimal Amount { get; set; }
    }
}
