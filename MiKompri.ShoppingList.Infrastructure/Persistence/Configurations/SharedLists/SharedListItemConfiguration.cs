using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Infrastructure.Persistence.Configurations.SharedLists
{
    public class SharedListItemConfiguration : IEntityTypeConfiguration<ListItem>
    {
        public void Configure(EntityTypeBuilder<ListItem> builder)
        {
            builder.Property(x => x.AddedBy).IsRequired();
            builder.Property(x => x.UpdatedBy).IsRequired();
        }
    }
}
