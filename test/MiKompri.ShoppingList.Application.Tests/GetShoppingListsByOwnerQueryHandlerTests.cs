using FluentAssertions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Queries.GetShoppingListsByOwner;
using MiKompri.ShoppingList.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.ShoppingList.Application.Tests
{
    public class GetShoppingListsByOwnerQueryHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Return_Lists_For_Given_Owner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();

            var first = new PurchaseList("Compra 1", ownerId, null);
            first.AddItem(new ListItem(Guid.NewGuid(), "Leche", 1.5m, 2));
            first.AddItem(new ListItem(Guid.NewGuid(), "Pan", 1m, 1));
            first.MarkItemAsPurchased(first.Items.First().ProductId);

            var second = new PurchaseList("Compra 2", ownerId, null);

            var lists = new List<PurchaseList> { first, second };

            var repoMock = new Mock<IPurchaseListRepository>();
            repoMock
                .Setup(r => r.GetByOwnerAsync(ownerId))
                .ReturnsAsync(lists);

            var handler = new GetShoppingListsByOwnerQueryHandler(repoMock.Object);

            var query = new GetShoppingListsByOwnerQuery(ownerId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Select(x => x.Name).Should().Contain(new[] { "Compra 1", "Compra 2" });
            result.Should().Contain(x =>
                x.Name == "Compra 1" &&
                x.TotalItems == 2 &&
                x.PurchasedItems == 1 &&
                x.PendingItems == 1 &&
                x.CompletionPercentage == 50);
            result.Should().Contain(x =>
                x.Name == "Compra 2" &&
                x.TotalItems == 0 &&
                x.PurchasedItems == 0 &&
                x.PendingItems == 0 &&
                x.CompletionPercentage == 0);

            repoMock.Verify(r => r.GetByOwnerAsync(ownerId), Times.Once);
        }
    }
}
