namespace MiKompri.ShoppingList.Api.Models
{
    public class CreatePurchaseListRequest
    {
       

        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid? GroupId { get; set; }
    }
}
