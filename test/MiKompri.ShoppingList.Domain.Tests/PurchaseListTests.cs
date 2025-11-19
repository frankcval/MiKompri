// Plan (pseudocódigo detallado):
// 1. Preparar pruebas unitarias para la clase `PurchaseList` usando xUnit.
// 2. Probar operaciones básicas de la entidad:
//    - Crear lista y renombrar -> verificar `Name` y `UpdatedAt` cambiado.
//    - Cambiar `GroupId` -> verificar `GroupId` y `UpdatedAt`.
//    - Actualizar `Description` -> verificar `Description` y `UpdatedAt`.
// 3. Probar gestión de items:
//    - `AddItem` agrega correctamente el item.
//    - `UpdateItem` actualiza nombre, precio y cantidad; lanzar excepción si no existe.
//    - `DeleteItem` elimina el item; lanzar excepción si no existe.
//    - `MarkItemAsPurchased` marca como comprado; lanzar excepción si no existe.
// 4. Probar `Progress` (porcentaje) tras marcar items como comprados.
// 5. En cada prueba comparar `UpdatedAt` con `CreatedAt` para verificar que se actualiza.
// 6. Usar constructores públicos de `ListItem` y las propiedades expuestas para aserciones.
// 7. Incluir pruebas para casos de error (excepciones esperadas).
//
// Implementación: crear clase de pruebas `PurchaseListTests` con métodos `[Fact]` para cada caso.

using System;
using System.Linq;
using Xunit;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests
{
    public class PurchaseListTests
    {
        [Fact]
        public void Rename_ActualizaNombreYUpdatedAt()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Compras", owner);
            var before = list.CreatedAt;

            list.Rename("Supermercado");

            Assert.Equal("Supermercado", list.Name);
            Assert.NotNull(list.UpdatedAt);
            Assert.True(list.UpdatedAt > before);
        }

        [Fact]
        public void ChangeGroup_ActualizaGroupIdYUpdatedAt()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);
            var before = list.CreatedAt;
            var newGroup = Guid.NewGuid();

            list.ChangeGroup(newGroup);

            Assert.Equal(newGroup, list.GroupId);
            Assert.NotNull(list.UpdatedAt);
            Assert.True(list.UpdatedAt > before);
        }

        [Fact]
        public void UpdateDescription_ActualizaDescripcionYUpdatedAt()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);
            var before = list.CreatedAt;

            list.UpdateDescription("Mi descripción");

            Assert.Equal("Mi descripción", list.Description);
            Assert.NotNull(list.UpdatedAt);
            Assert.True(list.UpdatedAt > before);
        }

        [Fact]
        public void AddItem_AgregaItem()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);

            var item = new ListItem(Guid.NewGuid(), "Leche", 1.25m, 2);

            list.AddItem(item);

            Assert.Single(list.Items);
            Assert.Contains(list.Items, i => i.ProductId == item.ProductId);
            Assert.NotNull(list.UpdatedAt);
        }

        [Fact]
        public void UpdateItem_ActualizaPropiedades()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);
            var item = new ListItem(Guid.NewGuid(), "Pan", 0.80m, 1);
            list.AddItem(item);

            list.UpdateItem(item.ProductId, "Pan Integral", 1.00m, 3);

            var updated = list.Items.First(i => i.ProductId == item.ProductId);
            Assert.Equal("Pan Integral", updated.Name);
            Assert.Equal(1.00m, updated.Price);
            Assert.Equal(3, updated.Quantity);
            Assert.NotNull(list.UpdatedAt);
        }

        [Fact]
        public void UpdateItem_LanzaSiNoExiste()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);

            Assert.Throws<InvalidOperationException>(() =>
                list.UpdateItem(Guid.NewGuid(), "X", null, null));
        }

        [Fact]
        public void DeleteItem_RemueveItem()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);
            var item = new ListItem(Guid.NewGuid(), "Queso", 2.5m, 1);
            list.AddItem(item);

            list.DeleteItem(item.ProductId);

            Assert.Empty(list.Items);
            Assert.NotNull(list.UpdatedAt);
        }

        [Fact]
        public void DeleteItem_LanzaSiNoExiste()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);

            Assert.Throws<InvalidOperationException>(() =>
                list.DeleteItem(Guid.NewGuid()));
        }

        [Fact]
        public void MarkItemAsPurchased_MarcaCorrectamente()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);
            var item = new ListItem(Guid.NewGuid(), "Huevos", 1.75m, 12);
            list.AddItem(item);

            list.MarkItemAsPurchased(item.ProductId);

            var updated = list.Items.First(i => i.ProductId == item.ProductId);
            Assert.True(updated.IsPurchased);
        }

        [Fact]
        public void MarkItemAsPurchased_LanzaSiNoExiste()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);

            Assert.Throws<ArgumentNullException>(() =>
                list.MarkItemAsPurchased(Guid.NewGuid()));
        }

        [Fact]
        public void Progress_ReflejaPorcentajeDeComprados()
        {
            var owner = Guid.NewGuid();
            var list = new PurchaseList("Lista", owner);
            var item1 = new ListItem(Guid.NewGuid(), "A", 1m, 1);
            var item2 = new ListItem(Guid.NewGuid(), "B", 2m, 1);
            list.AddItem(item1);
            list.AddItem(item2);

            // Marcar uno como comprado -> 1/2 = 50%
            list.MarkItemAsPurchased(item1.ProductId);

            // Se espera 50.00 (redondeado a 2 decimales según implementación)
            Assert.Equal(50.00, list.Progress.Percentage, 2);
        }
    }
}