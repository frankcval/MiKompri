using MiKompri.ShoppingList.Domain.Services;

namespace MiKompri.ShoppingList.Domain.Entities
{
    public class ItemExpenseRecord
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid SharedListItemId { get; private set; }
        public Guid PaidBy { get; private set; }
        public Guid? PurchasedBy { get; private set; }
        public decimal RealPaidPrice { get; private set; }
        public string Currency { get; private set; } = "EUR";
        public DateTime ExpenseDate { get; private set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        public List<ExpenseParticipant> Participants { get; private set; } = new();

        protected ItemExpenseRecord() { }

        public ItemExpenseRecord(Guid sharedListItemId, Guid paidBy, Guid? purchasedBy, decimal realPaidPrice, IEnumerable<Guid> participantUserIds, string? currency = null)
        {
            SharedListItemId = sharedListItemId;
            ApplyExpenseData(paidBy, purchasedBy, realPaidPrice, participantUserIds, currency);
        }

        public void Update(Guid paidBy, Guid? purchasedBy, decimal realPaidPrice, IEnumerable<Guid> participantUserIds, string? currency = null)
        {
            ApplyExpenseData(paidBy, purchasedBy, realPaidPrice, participantUserIds, currency);
            UpdatedAt = DateTime.UtcNow;
        }

        private void ApplyExpenseData(Guid paidBy, Guid? purchasedBy, decimal realPaidPrice, IEnumerable<Guid> participantUserIds, string? currency)
        {
            if (SharedListItemId == Guid.Empty)
                throw new InvalidOperationException("El item de la lista compartida es obligatorio.");
            if (paidBy == Guid.Empty)
                throw new InvalidOperationException("PaidBy es obligatorio.");
            if (realPaidPrice <= 0)
                throw new InvalidOperationException("El precio real pagado debe ser mayor que cero.");

            var participants = participantUserIds?.ToList() ?? new List<Guid>();

            if (participants.Any(x => x == Guid.Empty))
                throw new InvalidOperationException("Los participantes del gasto deben ser válidos.");
            if (participants.Count == 0)
                throw new InvalidOperationException("Se requiere al menos un participante activo para registrar el gasto.");
            if (participants.Count != participants.Distinct().Count())
                throw new InvalidOperationException("No se permiten participantes duplicados en un gasto.");

            PaidBy = paidBy;
            PurchasedBy = purchasedBy;
            RealPaidPrice = realPaidPrice;
            Currency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency.Trim().ToUpperInvariant();

            var shares = ExpenseShareRoundingPolicy.SplitWithPayerResidue(realPaidPrice, participants, paidBy);
            Participants.Clear();
            for (var i = 0; i < participants.Count; i++)
            {
                Participants.Add(new ExpenseParticipant(Id, participants[i], shares[i]));
            }
        }
    }
}
