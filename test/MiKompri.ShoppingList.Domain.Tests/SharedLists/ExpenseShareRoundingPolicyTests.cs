using MiKompri.ShoppingList.Domain.Services;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class ExpenseShareRoundingPolicyTests
    {
        [Fact]
        public void SplitWithPayerResidue_ShouldAssignResidueToPayer_WhenPayerIsParticipant()
        {
            var payer = Guid.NewGuid();
            var participants = new[] { payer, Guid.NewGuid(), Guid.NewGuid() };

            var shares = ExpenseShareRoundingPolicy.SplitWithPayerResidue(10m, participants, payer);

            Assert.Equal(3.34m, shares[0]);
            Assert.Equal(3.33m, shares[1]);
            Assert.Equal(3.33m, shares[2]);
            Assert.Equal(10m, shares.Sum());
        }

        [Fact]
        public void SplitWithPayerResidue_ShouldKeepConservation_WhenPayerIsNotParticipant()
        {
            var payer = Guid.NewGuid();
            var participants = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            var shares = ExpenseShareRoundingPolicy.SplitWithPayerResidue(10m, participants, payer);

            Assert.Equal(10m, shares.Sum());
        }
    }
}
