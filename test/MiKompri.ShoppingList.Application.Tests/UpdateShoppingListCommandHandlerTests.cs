// Plan detallado (pseudocódigo):
// 1. Preparar mocks para IPurchaseListRepository e IUnitOfWork usando Moq.
// 2. Caso 1: Lista existe
//    - Crear un PurchaseList válido usando su ctor público (nombre, ownerId, groupId opcional).
//    - Configurar repo.GetByIdAsync(listId) para devolver la lista existente.
//    - Crear UpdateShoppingListCommand con nuevo nombre y nuevo GroupId.
//    - Instanciar UpdateShoppingListCommandHandler con los mocks.
//    - Llamar a Handle(request, CancellationToken.None).
//    - Verificar que repo.UpdateAsync sea llamado exactamente una vez con un PurchaseList cuyo Name y GroupId coinciden con los nuevos valores.
//    - Verificar que unitOfWork.SaveChangesAsync sea llamado exactamente una vez.
// 3. Caso 2: Lista no existe
//    - Configurar repo.GetByIdAsync(listId) para devolver null.
//    - Llamar a Handle y comprobar que lanza KeyNotFoundException.
// 4. Mantener el test independiente, claro y reproducible.
// Nota: este archivo asume las rutas/espacios de nombres típicos; ajustar using si su solución difiere.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MiKompri.ShoppingList.Application.Commands.UpdateShoppingList;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain; // Ajustar si la entidad PurchaseList está en otro namespace
using MiKompri.ShoppingList.Domain.Entities; // Ajustar según su proyecto

namespace MiKompri.ShoppingList.Application.Tests.Commands.UpdateShoppingList
{
    public class UpdateShoppingListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenListExists_RenamesAndChangesGroupAndSaves()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var listId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var originalGroup = Guid.NewGuid();
            var existingList = new PurchaseList("OldName", ownerId, originalGroup);

            repoMock.Setup(r => r.GetByIdAsync(listId))
                    .ReturnsAsync(existingList);

            var handler = new UpdateShoppingListCommandHandler(repoMock.Object, uowMock.Object);

            var newGroup = Guid.NewGuid();
            var request = new UpdateShoppingListCommand(listId, "NewName", newGroup);
         

            // Act
            await handler.Handle(request, CancellationToken.None);

            // Assert
            repoMock.Verify(r => r.UpdateAsync(It.Is<PurchaseList>(l => l.Name == "NewName" && l.GroupId == newGroup)), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenListDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var listId = Guid.NewGuid();

            repoMock.Setup(r => r.GetByIdAsync(listId))
                    .ReturnsAsync((PurchaseList?)null);

            var handler = new UpdateShoppingListCommandHandler(repoMock.Object, uowMock.Object);

            var request = new UpdateShoppingListCommand(listId, "Any", null);
           

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await handler.Handle(request, CancellationToken.None));
            
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<PurchaseList>()), Times.Never);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}