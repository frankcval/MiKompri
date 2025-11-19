// Plan (pseudocódigo detallado):
// 1. Identificar por qué falla el test: la excepción indica que el mock esperaba GetByIdAsync ser invocado, pero el handler no lo llama.
// 2. Ajustar el test para reflejar el comportamiento real del handler:
//    - Si el handler sólo llama a DeleteAsync y luego a SaveChangesAsync, no debemos esperar GetByIdAsync.
//    - Mantener MockBehavior.Strict para detectar llamadas inesperadas, pero no establecer expectativas que no ocurren.
// 3. Implementar el test:
//    - Crear command con Guid aleatorio.
//    - Configurar el mock de IPurchaseListRepository para que espere DeleteAsync con el id y devuelva Task.CompletedTask.
//    - Configurar el mock de IUnitOfWork para que espere SaveChangesAsync y devuelva 1.
//    - Crear instancia de DeletePurchaseListCommandHandler con los mocks.
//    - Invocar Handle(command, CancellationToken.None) y await.
//    - Verificar que DeleteAsync y SaveChangesAsync fueron llamadas exactamente una vez.
// 4. Ejecutar y asegurarse de que no haya verificación de GetByIdAsync para evitar la Moq.MockException por ausencia de invocación.

// Nota: si en el futuro el handler cambia para cargar la entidad antes de borrar,
// se puede volver a añadir el Setup y la Verify de GetByIdAsync.

using MiKompri.ShoppingList.Application.Commands.DeleteShoppinList;
using MiKompri.ShoppingList.Application.Interfaces;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MiKompri.ShoppingList.Application.Tests.Commands.DeleteShoppinList
{
    public class DeletePurchaseListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCallRepositoryDeleteAndSaveChanges()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var command = new DeletePurchaseListCommand(listId);

            var repositoryMock = new Mock<IPurchaseListRepository>(MockBehavior.Strict);

            repositoryMock
                .Setup(r => r.DeleteAsync(listId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var handler = new DeletePurchaseListCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            repositoryMock.Verify(r => r.DeleteAsync(listId), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}