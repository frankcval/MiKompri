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
                Items = list.Items.Select(i => new ListItemDto
                {
                    Id = i.Id,
                    IsPurchased = i.IsPurchased,
                    ProducId = i.ProductId,
                    ProductName = i.Name,
                    ProductPrice = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}
