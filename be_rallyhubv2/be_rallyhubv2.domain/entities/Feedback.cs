using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Feedback : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? CourtId { get; set; }
    public Guid? BookingDetailId { get; set; }
    public FeedbackType Type { get; set; }
    public short Rating { get; set; } // 1..5
    public string Content { get; set; } = null!;

    // ===== Relationships =====
    public User? User { get; set; }
    public Court? Court { get; set; }
    public BookingDetail? BookingDetail { get; set; }
    public ICollection<FeedbackReply> Replies { get; set; } = new List<FeedbackReply>();
}
