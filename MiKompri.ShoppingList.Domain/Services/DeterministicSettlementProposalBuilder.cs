namespace MiKompri.ShoppingList.Domain.Services
{
    public sealed class DeterministicSettlementProposalBuilder
    {
        public IReadOnlyCollection<(Guid FromUserId, Guid ToUserId, decimal Amount)> Build(Dictionary<Guid, decimal> netBalances)
        {
            var debtors = netBalances
                .Where(x => x.Value < 0)
                .Select(x => (UserId: x.Key, Amount: Math.Abs(x.Value)))
                .OrderByDescending(x => x.Amount)
                .ThenBy(x => x.UserId)
                .ToList();

            var creditors = netBalances
                .Where(x => x.Value > 0)
                .Select(x => (UserId: x.Key, Amount: x.Value))
                .OrderByDescending(x => x.Amount)
                .ThenBy(x => x.UserId)
                .ToList();

            var transfers = new List<(Guid FromUserId, Guid ToUserId, decimal Amount)>();

            var d = 0;
            var c = 0;
            while (d < debtors.Count && c < creditors.Count)
            {
                var debt = debtors[d];
                var credit = creditors[c];

                var amount = Math.Min(debt.Amount, credit.Amount);
                amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

                if (amount > 0)
                {
                    transfers.Add((debt.UserId, credit.UserId, amount));
                }

                debtors[d] = (debt.UserId, debt.Amount - amount);
                creditors[c] = (credit.UserId, credit.Amount - amount);

                if (debtors[d].Amount <= 0.000001m) d++;
                if (creditors[c].Amount <= 0.000001m) c++;
            }

            return transfers;
        }

        public static int ComputeTransferUpperBound(Dictionary<Guid, decimal> netBalances)
        {
            var debtors = netBalances.Count(x => x.Value < 0);
            var creditors = netBalances.Count(x => x.Value > 0);
            return Math.Max(0, debtors + creditors - 1);
        }

        public static bool IsWithinTheoreticalLimit(Dictionary<Guid, decimal> netBalances, int transfersCount)
        {
            var upperBound = ComputeTransferUpperBound(netBalances);
            return transfersCount <= upperBound;
        }
    }
}
