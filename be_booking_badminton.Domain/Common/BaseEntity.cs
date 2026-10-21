namespace be_booking_badminton.Domain.Common;

public abstract class BaseEntity<TKey> : IAuditableEntity, ISoftDeletable
    where TKey : notnull
{
    protected BaseEntity()
    {
        if (typeof(TKey) == typeof(Guid))
        {
            Id = (TKey)(object)Guid.NewGuid();
        }
    }

    public TKey Id { get; protected set; } = default!;
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void MarkDeleted() => IsDeleted = true;

    public void Restore() => IsDeleted = false;
}
