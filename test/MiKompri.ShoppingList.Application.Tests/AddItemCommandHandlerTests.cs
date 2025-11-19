// Plan detallado (pseudocódigo):
// 1. Preparar mocks para IPurchaseListRepository e IUnitOfWork.
// 2. Caso exitoso:
//    - Crear una PurchaseList de prueba y hacer que repo.GetByIdAsync(devuelva la lista).
//    - Configurar IUnitOfWork.SaveChangesAsync para devolver 1.
//    - Ejecutar AddItemCommandHandler.Handle con un comando de ejemplo.
//    - Capturar el PurchaseList pasado a UpdateAsync y comprobar que se añadió un ListItem
//      con ProductId, Name, Price y Quantity esperados.
//    - Verificar que UpdateAsync y SaveChangesAsync fueron invocados exactamente una vez.
// 3. Caso lista no encontrada:
//    - Hacer que repo.GetByIdAsync(devuelva null.
//    - Ejecutar Handle y esperar una KeyNotFoundException.
// 4. Usar xUnit y Moq para implementar las verificaciones.
// 5. Mantener las pruebas independientes y deterministas.
//
// Implementación de pruebas unitarias:

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MiKompri.ShoppingList.Application.Commands.AddItemToList;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;
using System.Collections.Generic;

namespace MiKompri.ShoppingList.Application.Tests.Commands.AddItemToList
{
    public class AddItemCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldAddItemAndSave_WhenListExists()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var listId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var name = "Manzanas";
            decimal price = 2.5m;
            int quantity = 3;

            var purchaseList = new PurchaseList("Lista prueba", Guid.NewGuid());
            // Ensure SaveChangesAsync returns a completed result
            uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Capture the PurchaseList passed to UpdateAsync
            PurchaseList? capturedList = null;
            repoMock.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync(purchaseList);
            repoMock.Setup(r => r.UpdateAsync(It.IsAny<PurchaseList>()))
                    .Callback<PurchaseList>(pl => capturedList = pl)
                    .Returns(Task.CompletedTask);

            var handler = new AddItemCommandHandler(repoMock.Object, uowMock.Object);
            var command = new AddItemCommand(listId, productId, name, price, quantity); ;

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            repoMock.Verify(r => r.GetByIdAsync(listId), Times.Once);
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<PurchaseList>()), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(capturedList);

            // Verificar que el item fue añadido al agregado
            var added = capturedList!.Items?.FirstOrDefault(i => i.ProductId == productId);
            Assert.NotNull(added);
            Assert.Equal(name, added!.Name);
            Assert.Equal(price, added.Price);
            Assert.Equal(quantity, added.Quantity);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenListNotFound()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var listId = Guid.NewGuid();
            repoMock.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync((PurchaseList?)null);

            var handler = new AddItemCommandHandler(repoMock.Object, uowMock.Object);
            var command = new AddItemCommand(listId, Guid.NewGuid(), "Pera", 1.0m, 1); 

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));

            repoMock.Verify(r => r.GetByIdAsync(listId), Times.Once);
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<PurchaseList>()), Times.Never);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}