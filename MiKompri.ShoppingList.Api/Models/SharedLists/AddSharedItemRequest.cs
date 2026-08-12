namespace MiKompri.ShoppingList.Api.Models.SharedLists
{
    public class AddSharedItemRequest
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? EstimatedPrice { get; set; }
        public int Quantity { get; set; }
    }
}
