using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class ReportMessage : BaseEntity
{
    public Guid ReportId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset? ReadAt { get; set; }

    // ===== Relationships =====
    public Report? Report { get; set; }
    public User? Sender { get; set; }
}
