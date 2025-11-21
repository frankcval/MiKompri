using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Configurations
{
    public class ListItemConfiguration : IEntityTypeConfiguration<ListItem>
    {
        public void Configure(EntityTypeBuilder<ListItem> builder)
        {
            builder.ToTable("list_items");
            // PK del ítem
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id)
                   .ValueGeneratedOnAdd();   // Guid generado al insertar

            // ProductId puede repetirse en la tabla
            builder.Property(i => i.ProductId)
                   .IsRequired(); 

            builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.IsPurchased)
              .HasDefaultValue(false);

            builder.Property(i => i.PurchaseListId)       // 👈 mapeo explícito
         .IsRequired();

            // Relación con PurchaseList
            builder.HasOne(i => i.PurchaseList)
                   .WithMany(p => p.Items)
                   .HasForeignKey(i => i.PurchaseListId);

            // 🔒 Regla: un mismo producto no puede repetirse en la misma lista
            builder.HasIndex(i => new { i.PurchaseListId, i.ProductId })
                   .IsUnique();
        }
    }
}
