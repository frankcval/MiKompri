namespace MiKompri.ShoppingList.Application.DTOs
{
    public class SettlementDto
    {
        public SettlementSummaryDto Summary { get; set; } = new();
        public SettlementProposalDto Proposal { get; set; } = new();
    }
}
