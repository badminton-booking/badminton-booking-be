using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class FeedbackReply : BaseEntity
{
    public Guid FeedbackId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;

    // ===== Relationships =====
    public Feedback? Feedback { get; set; }
    public User? User { get; set; }
}
