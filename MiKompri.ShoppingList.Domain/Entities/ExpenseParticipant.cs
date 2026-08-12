namespace MiKompri.ShoppingList.Domain.Entities
{
    public class ExpenseParticipant
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ItemExpenseRecordId { get; private set; }
        public Guid ParticipantUserId { get; private set; }
        public decimal ShareAmount { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        protected ExpenseParticipant() { }

        public ExpenseParticipant(Guid itemExpenseRecordId, Guid participantUserId, decimal shareAmount)
        {
            if (itemExpenseRecordId == Guid.Empty)
                throw new InvalidOperationException("El gasto es obligatorio.");
            if (participantUserId == Guid.Empty)
                throw new InvalidOperationException("El participante es obligatorio.");

            ItemExpenseRecordId = itemExpenseRecordId;
            ParticipantUserId = participantUserId;
            ShareAmount = shareAmount;
        }
    }
}
