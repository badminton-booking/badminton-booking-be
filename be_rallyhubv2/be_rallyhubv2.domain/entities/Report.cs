using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Report : BaseEntity
{
    public Guid ReporterUserId { get; set; }
    public Guid? HandledByUserId { get; set; }
    public ReportTargetType TargetType { get; set; }
    public Guid? TargetId { get; set; } // polymorphic, khong FK vat ly
    public string Subject { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ReportStatus Status { get; set; } = ReportStatus.Open;
    public DateTimeOffset LastMessageAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    // ===== Relationships =====
    public User? Reporter { get; set; }
    public User? HandledByUser { get; set; }
    public ICollection<ReportMessage> Messages { get; set; } = new List<ReportMessage>();
}
