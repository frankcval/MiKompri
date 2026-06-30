// Plan (pseudoc�digo detallado):
// 1. Preparar mocks:
//    - Mock<IPurchaseListRepository>
//      * Capturar el PurchaseList pasado a AddAsync para poder inspeccionarlo.
//      * Configurar AddAsync para retornar Task.CompletedTask.
//    - Mock<IUnitOfWork>
//      * Configurar SaveChangesAsync para retornar 1 (simular �xito).
// 2. Crear instancia del handler `CreateShoppingListCommandHandler` con los mocks.
// 3. Construir el comando `CreateShoppingListCommand` con name, ownerId y groupId.
// 4. Llamar a handler.Handle(command, CancellationToken.None) y obtener el resultado (Guid).
// 5. Afirmaciones:
//    - El objeto pasado a AddAsync no es nulo.
//    - Sus propiedades Name, OwnerId y GroupId coinciden con las esperadas.
//    - El Guid devuelto por el handler coincide con `captured.Id`.
//    - Verificar que AddAsync y SaveChangesAsync fueron llamados exactamente una vez.
// 6. Probar comportamiento esperado en caso nominal (�xito).
//
// Nota: Este test usa xUnit y Moq. Ajustar referencias de NuGet en el proyecto de tests:
// - xunit
// - moq

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MiKompri.ShoppingList.Application.Commands.CreateShoppingList;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Tests.Commands.CreateShoppingList
{
    public class CreateShoppingListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Add_List_And_SaveChanges_Returns_ListId()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            PurchaseList? capturedList = null;

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PurchaseList>()))
                .Callback<PurchaseList>(p => capturedList = p)
                .Returns(Task.CompletedTask);

            var uowMock = new Mock<IUnitOfWork>();
            uowMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateShoppingListCommandHandler(repoMock.Object, uowMock.Object);

            var name = "Lista de Compra UnitTest";
            var ownerId = Guid.NewGuid();
            Guid? groupId = Guid.NewGuid();

            // Intentamos usar el ctor habitual; si la clase de comando difiere, adaptar aqu�.
            var command = new CreateShoppingListCommand(name, ownerId, groupId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedList);
            Assert.Equal(name, capturedList!.Name);
            Assert.Equal(ownerId, capturedList.OwnerId);
            Assert.Equal(groupId, capturedList.GroupId);
            Assert.Equal(capturedList.Id, result);

            repoMock.Verify(r => r.AddAsync(It.IsAny<PurchaseList>()), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_Name_Is_Invalid()
        {
            // Arrange
            var repoMock = new Mock<IPurchaseListRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var handler = new CreateShoppingListCommandHandler(repoMock.Object, uowMock.Object);
            var command = new CreateShoppingListCommand("   ", Guid.NewGuid(), null);

            // Act
            var act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(act);
            repoMock.Verify(r => r.AddAsync(It.IsAny<PurchaseList>()), Times.Never);
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}