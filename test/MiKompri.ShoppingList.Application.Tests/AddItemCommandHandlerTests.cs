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

            // fake resultado de SaveChangesAsync
            uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

            // capturar el ListItem que pasa el handler
            ListItem? capturedItem = null;

            repoMock
                .Setup(r => r.AddItemAsync(
                    listId,
                    It.IsAny<ListItem>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, ListItem, CancellationToken>((id, item, ct) =>
                {
                    capturedItem = item;
                })
                .Returns(Task.CompletedTask);

            var handler = new AddItemCommandHandler(repoMock.Object, uowMock.Object);
            var command = new AddItemCommand(listId, productId, name, price, quantity);

            // Act
            var resultId = await handler.Handle(command, CancellationToken.None);

            // Assert
            repoMock.Verify(r => r.AddItemAsync(
                                listId,
                                It.IsAny<ListItem>(),
                                It.IsAny<CancellationToken>()),
                            Times.Once);

            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                           Times.Once);

            Assert.NotNull(capturedItem);
            Assert.Equal(productId, capturedItem!.ProductId);
            Assert.Equal(name, capturedItem.Name);
            Assert.Equal(price, capturedItem.Price);
            Assert.Equal(quantity, capturedItem.Quantity);

            // el id devuelto por el handler debe ser el del item
            Assert.Equal(capturedItem.Id, resultId);
        }

        //Implementar el caso cuando la lista no existe
        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenListDoesNotExist()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();
            var listId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var name = "Manzanas";
            decimal price = 2.5m;
            int quantity = 3;
            // Configurar el mock para que devuelva null
            repoMock
                .Setup(r => r.AddItemAsync(
                    listId,
                    It.IsAny<ListItem>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Lista no existe"));
            var handler = new AddItemCommandHandler(repoMock.Object, uowMock.Object);
            var command = new AddItemCommand(listId, productId, name, price, quantity);
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));
        }
    }
    }