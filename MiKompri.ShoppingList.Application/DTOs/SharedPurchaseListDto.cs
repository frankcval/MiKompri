namespace MiKompri.ShoppingList.Application.DTOs
{
    public class SharedPurchaseListDto : PurchaseListDTO
    {
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public SharedListStatusDto Status { get; set; } = SharedListStatusDto.Active;
    }

    public enum SharedListStatusDto
    {
        Active = 1,
        Closed = 2,
        Archived = 3
    }
}
