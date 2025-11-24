/* 
Pseudocódigo detallado (plan):

1. Preparar mocks y datos de prueba:
   - Crear un Mock<IPurchaseListRepository>.
   - Generar dos GUIDs: listId y itemId.

2. Configurar comportamiento del mock para el caso exitoso:
   - Cuando DeleteItemAsync(listId, itemId) sea llamado, devolver Task.CompletedTask.

3. Crear una instancia de DeleteItemShoppingListCommandHandler pasando el mock.Object.

4. Crear una instancia de DeleteItemShoppingListCommand con los GUIDs (asumimos constructor (Guid listId, Guid itemId) o propiedades públicas).

5. Invocar Handle(command, CancellationToken.None) y esperar resultado.

6. Verificar:
   - El resultado es true.
   - El repositorio recibió una llamada a DeleteItemAsync con los parámetros esperados (Times.Once).

7. Caso de error:
   - Configurar el mock para que DeleteItemAsync lance una excepción.
   - Verificar que el handler propaga la excepción (Assert.ThrowsAsync).

Implementación:
- Usar xUnit para pruebas.
- Usar Moq para mocks.
- Mantener pruebas pequeñas y enfocadas.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MiKompri.ShoppingList.Application.Commands.DeleteItemShoppingList;
using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Application.Tests.Commands.DeleteItemShoppingList
{
    public class DeleteItemShoppingListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenCalled_DeletesItemAndReturnsTrue()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            var repoMock = new Mock<IPurchaseListRepository>();
            repoMock
                .Setup(r => r.DeleteItemAsync(listId, itemId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var uowMock = new Mock<IUnitOfWork>();
            uowMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new DeleteItemShoppingListCommandHandler(repoMock.Object, uowMock.Object);

            // Asumir constructor con (Guid listId, Guid itemId)
            var command = new DeleteItemShoppingListCommand(listId, itemId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            repoMock.Verify(r => r.DeleteItemAsync(listId, itemId), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ExceptionIsPropagated()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            var repoMock = new Mock<IPurchaseListRepository>();
            repoMock
                .Setup(r => r.DeleteItemAsync(listId, itemId))
                .ThrowsAsync(new InvalidOperationException("Repo failure"));

            var uowMock = new Mock<IUnitOfWork>();
            uowMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new DeleteItemShoppingListCommandHandler(repoMock.Object, uowMock.Object);
            var command = new DeleteItemShoppingListCommand(listId, itemId);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}