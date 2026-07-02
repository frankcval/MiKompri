namespace MiKompri.ShoppingList.Application.DTOs
{
    public class ListItemDto
    {
        public Guid Id { get; set; }
        public Guid ProducId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal? ProductPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsPurchased { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
