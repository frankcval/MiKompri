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

            var lists = new List<PurchaseList>
        {
            new("Compra 1", ownerId, null),
            new("Compra 2", ownerId, null)
        };

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

            repoMock.Verify(r => r.GetByOwnerAsync(ownerId), Times.Once);
        }
    }
}
