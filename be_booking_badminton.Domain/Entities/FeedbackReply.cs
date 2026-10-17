using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class FeedbackReply : BaseEntity<Guid>
{
    public FeedbackReply()
    {
        Id = Guid.NewGuid();
    }

    public Guid FeedbackId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentReplyId { get; set; }
    public string Content { get; set; } = string.Empty;

    public Feedback Feedback { get; set; } = null!;
    public User User { get; set; } = null!;
    public FeedbackReply? ParentReply { get; set; }
}
