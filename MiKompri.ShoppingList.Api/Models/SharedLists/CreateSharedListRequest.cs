namespace MiKompri.ShoppingList.Api.Models.SharedLists
{
    public class CreateSharedListRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid GroupId { get; set; }
    }
}
