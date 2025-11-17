namespace MiKompri.ShoppingList.Api.Models
{
    public class CreatePurchaseListRequest
    {
        private object listId;

        public CreatePurchaseListRequest(object listId)
        {
            this.listId = listId;
        }

        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid? GroupId { get; set; }
    }
}
