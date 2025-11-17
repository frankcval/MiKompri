namespace MiKompri.ShoppingList.Domain.Abtractions
{
    public abstract class Entity
    {
        //Todo refactor this

        public Guid Id { get; protected set; } = Guid.NewGuid();
    }
}
