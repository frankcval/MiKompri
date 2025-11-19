// Plan detallado (pseudocódigo):
// 1. Preparar mocks para IPurchaseListRepository e IUnitOfWork usando Moq.
// 2. Crear un PurchaseList válido (usando el constructor público conocido).
// 3. Crear un ListItem con el mismo productId que el itemId que vamos a marcar.
// 4. Añadir el ListItem a la PurchaseList usando el método AddItem para que exista el ítem a marcar.
// 5. Configurar el mock del repositorio para devolver la lista cuando se pida por Id.
// 6. Configurar los mocks para verificar que se llaman UpdateAsync y SaveChangesAsync.
// 7. Crear una instancia del handler con los mocks.
// 8. Construir el comando MarkItemAsPurchasedCommand con listId e itemId.
// 9. Llamar a handler.Handle(command, CancellationToken.None).
// 10. Verificar que repo.UpdateAsync y unitOfWork.SaveChangesAsync se llamaron exactamente una vez.
// 11. Verificar además que el item dentro de la lista quedó marcado (IsPurchased == true).
// 12. Probar caso negativo: cuando GetByIdAsync devuelve null, el handler debe lanzar KeyNotFoundException.

// Código de prueba unitaria
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MiKompri.ShoppingList.Application.Commands.MarkItemAsPurchased;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain;
using MiKompri.ShoppingList.Domain.Entities; // Ajustar según el namespace real del dominio si es necesario

namespace MiKompri.ShoppingList.Application.Tests.Commands.MarkItemAsPurchased
{
    public class MarkItemAsPurchasedCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_MarkItemAsPurchased_And_SaveChanges_When_ListFound()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var listId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            // Crear una instancia de PurchaseList usando el constructor conocido
            var purchaseList = new PurchaseList("Mi Lista", Guid.NewGuid());


            // Crear y añadir el ListItem que será marcado como comprado
            var listItem = new ListItem(itemId, "Producto de prueba", 9.99m, 1);
            purchaseList.AddItem(listItem);

            repoMock.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync(purchaseList);
            repoMock.Setup(r => r.UpdateAsync(purchaseList)).Returns(Task.CompletedTask).Verifiable();
            uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1).Verifiable();

            var handler = new MarkItemAsPurchasedCommandHandler(repoMock.Object, uowMock.Object);

            var command = new MarkItemAsPurchasedCommand(listId, itemId);

            // Act
            await handler.Handle((dynamic)command, CancellationToken.None);

            // Assert
            repoMock.Verify(r => r.UpdateAsync(purchaseList), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verificar que el item fue marcado como comprado
            var savedItem = purchaseList.Items.Single(i => i.ProductId == itemId);
            Assert.True(savedItem.IsPurchased);
        }

        [Fact]
        public async Task Handle_TryMarkItemAsPurchasedInNullList_ThrowKeyNotFoundException_When_ListNotFound()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var listId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            repoMock.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync((PurchaseList?)null);

            var handler = new MarkItemAsPurchasedCommandHandler(repoMock.Object, uowMock.Object);

            var command = new MarkItemAsPurchasedCommand(listId, itemId);
            
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await handler.Handle((dynamic)command, CancellationToken.None);
            });
        }
    }
}