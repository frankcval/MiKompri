namespace MiKompri.ShoppingList.Api.Models
{
    public class AddItemRequest
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
    }
}
