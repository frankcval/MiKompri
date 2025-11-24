// Plan (pseudocódigo detallado):
// 1. Crear un Mock de IPurchaseListRepository que acepte llamadas a UpdateItemAsync y devuelva Task.CompletedTask.
// 2. Instanciar UpdateItemShoopingListCommandHandler con el mock del repositorio.
// 3. Preparar un UpdateItemShoopingListCommand con valores de prueba (listId, ProdId, ProductName, Price, Quantity).
// 4. Llamar a Handle(...) del handler con CancellationToken.None y almacenar el resultado.
// 5. Verificar que el resultado sea true.
// 6. Verificar que UpdateItemAsync haya sido invocado exactamente una vez con los parámetros esperados.
//
// Notas:
// - Usamos xUnit para las pruebas y Moq para el mock del repositorio.
// - El test es asíncrono y mínimo: valida que el handler delega correctamente la actualización al repositorio.

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Application.Commands.UpdateItemShoppingList;

namespace MiKompri.ShoppingList.Application.Tests.Commands.UpdateItemShoppingList
{
    public class UpdateItemShoopingListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CallsUpdateItemAsync_AndReturnsTrue()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>(MockBehavior.Strict);
            repoMock
                .Setup(r => r.UpdateItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<int?>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var uowMock = new Mock<IUnitOfWork>();
            uowMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new UpdateItemShoopingListCommandHandler(repoMock.Object, uowMock.Object);

            var listId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            // Se asume que UpdateItemShoopingListCommand tiene propiedades públicas compatibles con las usadas en el handler.
            var request = new UpdateItemShoopingListCommand(listId, prodId, "Manzanas", 1.5m, 3);
           

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result);
            repoMock.Verify(r => r.UpdateItemAsync(listId, prodId, "Manzanas", 1.5m, 3), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}