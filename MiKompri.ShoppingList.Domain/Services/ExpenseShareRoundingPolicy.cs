namespace MiKompri.ShoppingList.Domain.Services
{
    public static class ExpenseShareRoundingPolicy
    {
        public static List<decimal> SplitWithPayerResidue(decimal total, IReadOnlyList<Guid> participants, Guid paidBy)
        {
            if (participants.Count == 0)
                throw new InvalidOperationException("Debe existir al menos un participante.");
            if (total <= 0)
                throw new InvalidOperationException("El total del gasto debe ser mayor que cero.");

            var baseShare = Math.Round(total / participants.Count, 2, MidpointRounding.AwayFromZero);
            var shares = participants.Select(_ => baseShare).ToList();
            var residue = total - shares.Sum();

            if (residue != 0)
            {
                var payerIndex = -1;
                for (var i = 0; i < participants.Count; i++)
                {
                    if (participants[i] == paidBy)
                    {
                        payerIndex = i;
                        break;
                    }
                }

                var targetIndex = payerIndex >= 0 ? payerIndex : 0;
                shares[targetIndex] += residue;
            }

            return shares;
        }
    }
}
