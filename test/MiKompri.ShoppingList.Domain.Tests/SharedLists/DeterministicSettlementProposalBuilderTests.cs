using MiKompri.ShoppingList.Domain.Services;

namespace MiKompri.ShoppingList.Domain.Tests.SharedLists
{
    public class DeterministicSettlementProposalBuilderTests
    {
        [Fact]
        public void Build_WithSameInput_ShouldReturnDeterministicTransfers()
        {
            var builder = new DeterministicSettlementProposalBuilder();
            var balances = new Dictionary<Guid, decimal>
            {
                [Guid.Parse("11111111-1111-1111-1111-111111111111")] = -30m,
                [Guid.Parse("22222222-2222-2222-2222-222222222222")] = -10m,
                [Guid.Parse("33333333-3333-3333-3333-333333333333")] = 25m,
                [Guid.Parse("44444444-4444-4444-4444-444444444444")] = 15m
            };

            var first = builder.Build(balances).ToList();
            var second = builder.Build(balances).ToList();

            Assert.Equal(first.Count, second.Count);
            Assert.Equal(first, second);
            Assert.True(DeterministicSettlementProposalBuilder.IsWithinTheoreticalLimit(balances, first.Count));
        }
    }
}
