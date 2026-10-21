using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class Report : BaseEntity<Guid>
{
    public Guid ReporterUserId { get; set; }
    public Guid? HandledByUserId { get; set; }
    public ReportTargetType TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportStatus Status { get; set; } = ReportStatus.Open;
    public DateTimeOffset LastMessageAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    public User ReporterUser { get; set; } = null!;
    public User? HandledByUser { get; set; }
}
