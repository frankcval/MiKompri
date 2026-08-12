using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Configurations.SharedLists
{
    public class ExpenseParticipantConfiguration : IEntityTypeConfiguration<ExpenseParticipant>
    {
        public void Configure(EntityTypeBuilder<ExpenseParticipant> builder)
        {
            builder.ToTable("shared_list_expense_participants");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.ItemExpenseRecordId).IsRequired();
            builder.Property(x => x.ParticipantUserId).IsRequired();
            builder.Property(x => x.ShareAmount).HasColumnType("decimal(12,2)").IsRequired();
            builder.HasIndex(x => new { x.ItemExpenseRecordId, x.ParticipantUserId }).IsUnique();
        }
    }
}
