namespace MiKompri.ShoppingList.Api.Models
{
    public class UpdateItemRequest
    {
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }
}
