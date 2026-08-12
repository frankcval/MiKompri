namespace MiKompri.ShoppingList.Domain.Services
{
    public static class ExpenseSettlementCalculator
    {
        public static Dictionary<Guid, decimal> ComputeNetBalances(
            IEnumerable<(Guid PaidBy, decimal Amount)> payments,
            IEnumerable<(Guid ParticipantUserId, decimal ShareAmount)> shares)
        {
            var result = new Dictionary<Guid, decimal>();

            foreach (var payment in payments)
            {
                if (!result.ContainsKey(payment.PaidBy))
                    result[payment.PaidBy] = 0m;

                result[payment.PaidBy] += payment.Amount;
            }

            foreach (var share in shares)
            {
                if (!result.ContainsKey(share.ParticipantUserId))
                    result[share.ParticipantUserId] = 0m;

                result[share.ParticipantUserId] -= share.ShareAmount;
            }

            return result;
        }
    }
}
