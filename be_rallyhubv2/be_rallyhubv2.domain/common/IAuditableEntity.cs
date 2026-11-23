namespace be_rallyhubv2.domain.common;

public interface IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}