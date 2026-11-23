using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Data { get; set; } // jsonb
    public DateTimeOffset? ReadAt { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
}
