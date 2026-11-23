namespace be_rallyhubv2.domain.common;

public abstract class BaseEntity: IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}