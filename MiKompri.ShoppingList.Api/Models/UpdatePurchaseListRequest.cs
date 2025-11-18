namespace MiKompri.ShoppingList.Api.Models
{
    public class UpdatePurchaseListRequest
    {
        public Guid? PurchaseId { get; set; }      
        public string? Name { get; set; }
        public Guid? GroupId { get; set; }        
    }
}
