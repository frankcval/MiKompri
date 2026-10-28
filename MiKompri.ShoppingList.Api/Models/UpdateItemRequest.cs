namespace MiKompri.ShoppingList.Api.Models
{
    public class UpdateItemRequest
    {
        public string? ProductName { get; set; }
        private decimal? _price;
        public decimal? Price { get => _price; set { _price = value; PriceSpecified = true; } }
        [System.Text.Json.Serialization.JsonIgnore]
        public bool PriceSpecified { get; private set; }
        public int? Quantity { get; set; }
    }
}
