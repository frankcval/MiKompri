namespace MiKompri.ShoppingList.Application.DTOs
{
    public class PurchaseListDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }

        public Guid? GroupId { get; set; }
        public double CompletionPercentage { get; set; }

        public List<ListItemDto> Items { get; set; } = new();
    }
}
