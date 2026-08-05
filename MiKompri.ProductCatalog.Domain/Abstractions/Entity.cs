namespace MiKompri.ProductCatalog.Domain.Abstractions;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    protected Entity()
    {
        var now = DateTime.UtcNow;

        Id = Guid.NewGuid();
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch(Guid? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    protected void SetAuditUsers(Guid? createdBy, Guid? updatedBy)
    {
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }
}
