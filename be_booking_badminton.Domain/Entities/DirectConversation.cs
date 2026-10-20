using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public class DirectConversation : BaseEntity<Guid>
{
    public Guid StartedByUserId { get; set; }
    public User? StartedByUser { get; set; }
    public Guid OtherUserId { get; set; }
    public User? OtherUser { get; set; }
    public DateTimeOffset LastMessageAt { get; set; }
    /// <summary>Số thứ tự của tin cuối cùng trong cuộc chat riêng; bắt đầu từ 1 vì conversation được tạo cùng tin đầu tiên.</summary>
    public long LastMessageSequence { get; set; }

    public ICollection<DirectMessage> Messages { get; set; } = new List<DirectMessage>();
}
