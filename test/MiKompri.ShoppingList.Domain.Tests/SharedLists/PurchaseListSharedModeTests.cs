using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class PurchaseListSharedModeTests
    {
        [Fact]
        public void Constructor_WithGroupId_ShouldCreateSharedListInActiveStatus()
        {
            var list = new PurchaseList("Lista compartida", Guid.NewGuid(), Guid.NewGuid());

            Assert.True(list.IsShared);
            Assert.Equal(SharedListStatus.Active, list.Status);
        }

        [Fact]
        public void CloseSharedList_WhenPersonalList_ShouldThrow()
        {
            var list = new PurchaseList("Lista personal", Guid.NewGuid());

            Assert.Throws<InvalidOperationException>(() => list.CloseSharedList());
        }

        [Fact]
        public void CloseSharedList_WhenSharedList_ShouldPreventNewItems()
        {
            var list = new PurchaseList("Lista compartida", Guid.NewGuid(), Guid.NewGuid());
            list.CloseSharedList();

            Assert.Equal(SharedListStatus.Closed, list.Status);

            var item = new ListItem(Guid.NewGuid(), "Leche", 1.50m, 1, Guid.NewGuid());
            Assert.Throws<InvalidOperationException>(() => list.AddItem(item));
        }
    }
}
