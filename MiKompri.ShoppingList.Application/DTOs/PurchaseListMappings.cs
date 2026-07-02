using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.DTOs
{
    internal static class PurchaseListMappings
    {
        public static PurchaseListDTO ToDto(this PurchaseList list)
        {
            return new PurchaseListDTO
            {
                Id = list.Id,
                Name = list.Name,
                OwnerId = list.OwnerId,
                GroupId = list.GroupId,
                TotalItems = list.Progress.TotalItems,
                PurchasedItems = list.Progress.PurchasedItems,
                PendingItems = list.Progress.PendingItems,
                CompletionPercentage = list.Progress.Percentage,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                Items = list.Items.Select(i => i.ToDto()).ToList()
            };
        }

        public static ListItemDto ToDto(this ListItem item)
        {
            return new ListItemDto
            {
                Id = item.Id,
                IsPurchased = item.IsPurchased,
                ProducId = item.ProductId,
                ProductName = item.Name,
                ProductPrice = item.Price,
                Quantity = item.Quantity,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }
    }
}
