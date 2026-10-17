using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class ReportMessage : BaseEntity<Guid>
{
    public ReportMessage()
    {
        Id = Guid.NewGuid();
    }

    public Guid ReportId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset? ReadAt { get; set; }

    public Report Report { get; set; } = null!;
    public User SenderUser { get; set; } = null!;
}
